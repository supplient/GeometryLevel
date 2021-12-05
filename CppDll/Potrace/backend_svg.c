/* Copyright (C) 2001-2019 Peter Selinger.
   This file is part of Potrace. It is free software and it is covered
   by the GNU General Public License. See the file COPYING for details. */


/* The SVG backend of Potrace. */

#include "config.h"

#include <stdio.h>
#include <stdarg.h>
#include <string.h>
#include <math.h>

#include "potracelib.h"
#include "curve.h"
#include "main.h"
#include "backend_svg.h"
#include "lists.h"
#include "auxiliary.h"

/* ---------------------------------------------------------------------- */
/* string operating functions */
static int outed = 0;
static int outMaxLen = 0;

#define OUTPUT(x, ...) { \
  outed+=sprintf(sout+outed, x, ##__VA_ARGS__); \
  if(outed>outMaxLen) { \
    fprintf(stderr, "" POTRACE ": Too small output buffer\n"); \
    exit(2); \
  } \
}

/* ---------------------------------------------------------------------- */
/* path-drawing auxiliary functions */

/* coordinate quantization */
static inline point_t unit(dpoint_t p) {
  point_t q;

  q.x = (long)(floor(p.x*info.unit+.5));
  q.y = (long)(floor(p.y*info.unit+.5));
  return q;
}

static point_t cur;
static char lastop = 0;
static int column = 0;
static int newline = 1;

static void shiptoken(char *sout, const char *token) {
  int c = strlen(token);
  if (!newline && column+c+1 > 75) {
    OUTPUT( "\n");
    column = 0;
    newline = 1;
  } else if (!newline) {
    OUTPUT( " ");
    column++;
  }
  OUTPUT( "%s", token);
  column += c;
  newline = 0;
}

static void ship(char *sout, const char *fmt, ...) {
  va_list args;
  static char buf[4096]; /* static string limit is okay here because
			    we only use constant format strings - for
			    the same reason, it is okay to use
			    vsprintf instead of vsnprintf below. */
  char *p, *q;

  va_start(args, fmt);
  vsprintf(buf, fmt, args);
  buf[4095] = 0;
  va_end(args);

  p = buf;
  while ((q = strchr(p, ' ')) != NULL) {
    *q = 0;
    shiptoken(sout, p);
    p = q+1;
  }
  shiptoken(sout, p);
}

static void svg_moveto(char *sout, dpoint_t p) {
  cur = unit(p);

  ship(sout, "M%ld %ld", cur.x, cur.y);
  lastop = 'M';
}

static void svg_rmoveto(char *sout, dpoint_t p) {
  point_t q;

  q = unit(p);
  ship(sout, "m%ld %ld", q.x-cur.x, q.y-cur.y);
  cur = q;
  lastop = 'm';
}

static void svg_lineto(char *sout, dpoint_t p) {
  point_t q;

  q = unit(p);

  if (lastop != 'l') {
    ship(sout, "l%ld %ld", q.x-cur.x, q.y-cur.y);
  } else {
    ship(sout, "%ld %ld", q.x-cur.x, q.y-cur.y);
  }
  cur = q;
  lastop = 'l';
}

static void svg_curveto(char *sout, dpoint_t p1, dpoint_t p2, dpoint_t p3) {
  point_t q1, q2, q3;

  q1 = unit(p1);
  q2 = unit(p2);
  q3 = unit(p3);

  if (lastop != 'c') {
    ship(sout, "c%ld %ld %ld %ld %ld %ld", q1.x-cur.x, q1.y-cur.y, q2.x-cur.x, q2.y-cur.y, q3.x-cur.x, q3.y-cur.y);
  } else {
    ship(sout, "%ld %ld %ld %ld %ld %ld", q1.x-cur.x, q1.y-cur.y, q2.x-cur.x, q2.y-cur.y, q3.x-cur.x, q3.y-cur.y);
  }
  cur = q3;
  lastop = 'c';
}

/* ---------------------------------------------------------------------- */
/* functions for converting a path to an SVG path element */

/* Explicit encoding. If abs is set, move to first coordinate
   absolutely. */
static int svg_path(char *sout, potrace_curve_t *curve, int abs) {
  int i;
  dpoint_t *c;
  int m = curve->n;

  c = curve->c[m-1];
  if (abs) {
    svg_moveto(sout, c[2]);
  } else {
    svg_rmoveto(sout, c[2]);
  }

  for (i=0; i<m; i++) {
    c = curve->c[i];
    switch (curve->tag[i]) {
    case POTRACE_CORNER:
      svg_lineto(sout, c[1]);
      svg_lineto(sout, c[2]);
      break;
    case POTRACE_CURVETO:
      svg_curveto(sout, c[0], c[1], c[2]);
      break;
    }
  }
  newline = 1;
  shiptoken(sout, "z");
  return 0;
}

/* produce a jaggy path - for debugging. If abs is set, move to first
   coordinate absolutely. If abs is not set, move to first coordinate
   relatively, and traverse path in the opposite direction. */
static int svg_jaggy_path(char *sout, point_t *pt, int n, int abs) {
  int i;
  point_t cur, prev;
  
  if (abs) {
    cur = prev = pt[n-1];
    svg_moveto(sout, dpoint(cur));
    for (i=0; i<n; i++) {
      if (pt[i].x != cur.x && pt[i].y != cur.y) {
	cur = prev;
	svg_lineto(sout, dpoint(cur));
      }
      prev = pt[i];
    }
    svg_lineto(sout, dpoint(pt[n-1]));
  } else {
    cur = prev = pt[0];
    svg_rmoveto(sout, dpoint(cur));
    for (i=n-1; i>=0; i--) {
      if (pt[i].x != cur.x && pt[i].y != cur.y) {
	cur = prev;
	svg_lineto(sout, dpoint(cur));
      }
      prev = pt[i];
    }
    svg_lineto(sout, dpoint(pt[0]));
  }
  newline = 1;
  shiptoken(sout, "z");
  return 0;
}

static void write_paths_opaque(char *sout, potrace_path_t *tree) {
  potrace_path_t *p, *q;

  for (p=tree; p; p=p->sibling) {
    if (info.grouping == 2) {
      OUTPUT( "<g>\n");
      OUTPUT( "<g>\n");
    }
    OUTPUT( "<path fill=\"#%06x\" stroke=\"none\" d=\"", info.color);
    column = outed;
    newline = 1;
    lastop = 0;
    if (info.debug == 1) {
      svg_jaggy_path(sout, p->priv->pt, p->priv->len, 1);
    } else {
      svg_path(sout, &p->curve, 1);
    }
    OUTPUT( "\"/>\n");
    for (q=p->childlist; q; q=q->sibling) {
      OUTPUT( "<path fill=\"#%06x\" stroke=\"none\" d=\"", info.fillcolor);
      column = outed;
      newline = 1;
      lastop = 0;
      if (info.debug == 1) {
	svg_jaggy_path(sout, q->priv->pt, q->priv->len, 1);
      } else {
	svg_path(sout, &q->curve, 1);
      }
      OUTPUT( "\"/>\n");
    }
    if (info.grouping == 2) {
      OUTPUT( "</g>\n");
    }
    for (q=p->childlist; q; q=q->sibling) {
      write_paths_opaque(sout, q->childlist);
    }
    if (info.grouping == 2) {
      OUTPUT( "</g>\n");
    }
  }
}

static void write_paths_transparent_rec(char *sout, potrace_path_t *tree) {
  potrace_path_t *p, *q;

  for (p=tree; p; p=p->sibling) {
    if (info.grouping == 2) {
      OUTPUT( "<g>\n");
    }
    if (info.grouping != 0) {
      OUTPUT( "<path d=\"");
      column = outed;
      newline = 1;
      lastop = 0;
    }
    if (info.debug == 1) {
      svg_jaggy_path(sout, p->priv->pt, p->priv->len, 1);
    } else {
      svg_path(sout, &p->curve, 1);
    }
    for (q=p->childlist; q; q=q->sibling) {
      if (info.debug == 1) {
	svg_jaggy_path(sout, q->priv->pt, q->priv->len, 0);
      } else {
	svg_path(sout, &q->curve, 0);
      }
    }
    if (info.grouping != 0) {
      OUTPUT( "\"/>\n");
    }
    for (q=p->childlist; q; q=q->sibling) {
      write_paths_transparent_rec(sout, q->childlist);
    }
    if (info.grouping == 2) {
      OUTPUT( "</g>\n");
    }
  }
}

static void write_paths_transparent(char *sout, potrace_path_t *tree) {
  if (info.grouping == 0) {
    OUTPUT( "<path d=\"");
    column = outed;
    newline = 1;
    lastop = 0;
  }
  write_paths_transparent_rec(sout, tree);
  if (info.grouping == 0) {
    OUTPUT( "\"/>\n");
  }
}

/* ---------------------------------------------------------------------- */
/* Backend. */

/* public interface for SVG */
int page_svg(char *sout, unsigned int _outMaxLen, potrace_path_t *plist, imginfo_t *imginfo) {

  double bboxx = imginfo->trans.bb[0]+imginfo->lmar+imginfo->rmar;
  double bboxy = imginfo->trans.bb[1]+imginfo->tmar+imginfo->bmar;
  double origx = imginfo->trans.orig[0] + imginfo->lmar;
  double origy = bboxy - imginfo->trans.orig[1] - imginfo->bmar;
  double scalex = imginfo->trans.scalex / info.unit;
  double scaley = -imginfo->trans.scaley / info.unit;

  outed = 0;
  outMaxLen = _outMaxLen;

  /* header */
  // OUTPUT( "<?xml version=\"1.0\" standalone=\"no\"?>\n");
  // OUTPUT( "<!DOCTYPE svg PUBLIC \"-//W3C//DTD SVG 20010904//EN\"\n");
  // OUTPUT( " \"http://www.w3.org/TR/2001/REC-SVG-20010904/DTD/svg10.dtd\">\n");

  /* set bounding box and namespace */
  OUTPUT("<svg version=\"1.0\" xmlns=\"http://www.w3.org/2000/svg\"\n");
  OUTPUT(" width=\"%fpt\" height=\"%fpt\" viewBox=\"0 0 %f %f\"\n", 
	  bboxx, bboxy, bboxx, bboxy);
  OUTPUT( " preserveAspectRatio=\"xMidYMid meet\">\n");

  /* metadata: creator */
  OUTPUT( "<metadata>\n");
  OUTPUT( "Created by " POTRACE " " VERSION ", written by Peter Selinger 2001-2019\n");
  OUTPUT( "</metadata>\n");

  /* use a "group" tag to establish coordinate system and style */
  OUTPUT( "<g transform=\"");
  if (origx != 0 || origy != 0) {
    OUTPUT( "translate(%f,%f) ", origx, origy);
  }
  if (info.angle != 0) {
    OUTPUT( "rotate(%.2f) ", -info.angle);
  }
  OUTPUT( "scale(%f,%f)", scalex, scaley);
  OUTPUT( "\"\n");
  OUTPUT( "fill=\"#%06x\" stroke=\"none\">\n", info.color);

  if (info.opaque) {
    write_paths_opaque(sout, plist);
  } else {
    write_paths_transparent(sout, plist);
  }

  /* write footer */
  OUTPUT( "</g>\n");
  OUTPUT( "</svg>\n");

  return 0;
}

