#include "config.h"

#include <stdio.h>
#include <stdlib.h>
#include <errno.h>
#include <string.h>
#include <math.h>

#include "main.h"
#include "potracelib.h"
#include "backend_svg.h"
#include "potracelib.h"
#include "bitmap_io.h"
#include "bitmap.h"
#include "auxiliary.h"
#include "trans.h"

#ifndef M_PI
#define M_PI 3.14159265358979323846
#endif

#define UNDEF ((double)(1e30))   /* a value to represent "undefined" */

struct info_s info;

/* ---------------------------------------------------------------------- */
/* backends and their characteristics */
struct backend_s {
  const char *name;       /* name of this backend */
  const char *ext;        /* file extension */
  int fixed;        /* fixed page size backend? */
  int pixel;        /* pixel-based backend? */
  int multi;        /* multi-page backend? */
  int (*init_f)(FILE *fout);                 /* initialization function */
  int (*page_f)(char *sout, unsigned int outMaxLen, potrace_path_t *plist, imginfo_t *imginfo);
                                             /* per-bitmap function */
  int (*term_f)(FILE *fout);                 /* finalization function */
  int opticurve;    /* opticurve capable (true Bezier curves?) */
};
typedef struct backend_s backend_t;  

static backend_t backend[] = {
  { "svg",        ".svg", 0, 0, 0,   NULL,     page_svg,     NULL,     1 },
  { NULL, NULL, 0, 0, 0, NULL, NULL, NULL, 0 },
};

/* look up a backend by name. If found, return 0 and set *bp. If not
   found leave *bp unchanged and return 1, or 2 on ambiguous
   prefix. */

static inline double double_of_dim(dim_t d, double def) {
  if (d.d) {
    return d.x * d.d;
  } else {
    return d.x * def;
  }
}

/* ---------------------------------------------------------------------- */
/* option processing */

static void dopts() {
  /* defaults */
  info.debug = 0;
  info.width_d.x = UNDEF;
  info.height_d.x = UNDEF;
  info.rx = UNDEF;
  info.ry = UNDEF;
  info.sx = UNDEF;
  info.sy = UNDEF;
  info.stretch = 1;
  info.lmar_d.x = UNDEF;
  info.rmar_d.x = UNDEF;
  info.tmar_d.x = UNDEF;
  info.bmar_d.x = UNDEF;
  info.angle = 0;
  info.paperwidth = DEFAULT_PAPERWIDTH;
  info.paperheight = DEFAULT_PAPERHEIGHT;
  info.tight = 0;
  info.unit = 10;
  info.compress = 1;
  info.pslevel = 2;
  info.color = 0x000000;
  info.gamma = 2.2;
  info.param = potrace_param_default();
  if (!info.param) {
    fprintf(stderr, "" POTRACE ": %s\n", strerror(errno));
    exit(2);
  }
  info.longcoding = 0;
  info.blacklevel = 0.5;
  info.invert = 0;
  info.opaque = 0;
  info.grouping = 1;
  info.fillcolor = 0xffffff;
  info.progress = 0;

  static char *infileName = "d.bmp"; // DEBUG
  static char *outfileName = "d.svg";
  info.infiles = &infileName;
  info.infilecount = 1;
  info.some_infiles = 1;
  info.outfile = outfileName;
}

/* ---------------------------------------------------------------------- */
/* calculations with bitmap dimensions, positioning etc */

/* determine the dimensions of the output based on command line and
   image dimensions, and optionally, based on the actual image outline. */
static void calc_dimensions(imginfo_t *imginfo, potrace_path_t *plist) {
  double dim_def;
  double maxwidth, maxheight, sc;
  int default_scaling = 0;

  /* we take care of a special case: if one of the image dimensions is
     0, we change it to 1. Such an image is empty anyway, so there
     will be 0 paths in it. Changing the dimensions avoids division by
     0 error in calculating scaling factors, bounding boxes and
     such. This doesn't quite do the right thing in all cases, but it
     is better than causing overflow errors or "nan" output in
     backends.  Human users don't tend to process images of size 0
     anyway; they might occur in some pipelines. */
  if (imginfo->pixwidth == 0) {
    imginfo->pixwidth = 1;
  }
  if (imginfo->pixheight == 0) {
    imginfo->pixheight = 1;
  }

  /* set the default dimension for width, height, margins */
  if (info.backend->pixel) {
    dim_def = DIM_PT;
  } else {
    dim_def = DEFAULT_DIM;
  }

  /* apply default dimension to width, height, margins */
  imginfo->width = info.width_d.x == UNDEF ? UNDEF : double_of_dim(info.width_d, dim_def);
  imginfo->height = info.height_d.x == UNDEF ? UNDEF : double_of_dim(info.height_d, dim_def);
  imginfo->lmar = info.lmar_d.x == UNDEF ? UNDEF : double_of_dim(info.lmar_d, dim_def);
  imginfo->rmar = info.rmar_d.x == UNDEF ? UNDEF : double_of_dim(info.rmar_d, dim_def);
  imginfo->tmar = info.tmar_d.x == UNDEF ? UNDEF : double_of_dim(info.tmar_d, dim_def);
  imginfo->bmar = info.bmar_d.x == UNDEF ? UNDEF : double_of_dim(info.bmar_d, dim_def);

  /* start with a standard rectangle */
  trans_from_rect(&imginfo->trans, imginfo->pixwidth, imginfo->pixheight);

  /* if info.tight is set, tighten the bounding box */
  if (info.tight) {
    trans_tighten(&imginfo->trans, plist);
  }

  /* sx/rx is just an alternate way to specify width; sy/ry is just an
     alternate way to specify height. */
  if (info.backend->pixel) {
    if (imginfo->width == UNDEF && info.sx != UNDEF) {
      imginfo->width = imginfo->trans.bb[0] * info.sx;
    }
    if (imginfo->height == UNDEF && info.sy != UNDEF) {
      imginfo->height = imginfo->trans.bb[1] * info.sy;
    }
  } else {
    if (imginfo->width == UNDEF && info.rx != UNDEF) {
      imginfo->width = imginfo->trans.bb[0] / info.rx * 72;
    }
    if (imginfo->height == UNDEF && info.ry != UNDEF) {
      imginfo->height = imginfo->trans.bb[1] / info.ry * 72;
    }
  }

  /* if one of width/height is specified, use stretch to determine the
     other */
  if (imginfo->width == UNDEF && imginfo->height != UNDEF) {
    imginfo->width = imginfo->height / imginfo->trans.bb[1] * imginfo->trans.bb[0] / info.stretch;
  } else if (imginfo->width != UNDEF && imginfo->height == UNDEF) {
    imginfo->height = imginfo->width / imginfo->trans.bb[0] * imginfo->trans.bb[1] * info.stretch;
  }

  /* if width and height are still variable, tenatively use the
     default scaling factor of 72dpi (for dimension-based backends) or
     1 (for pixel-based backends). For fixed-size backends, this will
     be adjusted later to fit the page. */
  if (imginfo->width == UNDEF && imginfo->height == UNDEF) {
    imginfo->width = imginfo->trans.bb[0];
    imginfo->height = imginfo->trans.bb[1] * info.stretch;
    default_scaling = 1;
  } 

  /* apply scaling */
  trans_scale_to_size(&imginfo->trans, imginfo->width, imginfo->height);

  /* apply rotation, and tighten the bounding box again, if necessary */
  if (info.angle != 0.0) {
    trans_rotate(&imginfo->trans, info.angle);
    if (info.tight) {
      trans_tighten(&imginfo->trans, plist);
    }
  }

  /* for fixed-size backends, if default scaling was in effect,
     further adjust the scaling to be the "best fit" for the given
     page size and margins. */
  if (default_scaling && info.backend->fixed) {
    
    /* try to squeeze it between margins */
    maxwidth = UNDEF;
    maxheight = UNDEF;
    
    if (imginfo->lmar != UNDEF && imginfo->rmar != UNDEF) {
      maxwidth = info.paperwidth - imginfo->lmar - imginfo->rmar;
    } 
    if (imginfo->bmar != UNDEF && imginfo->tmar != UNDEF) {
      maxheight = info.paperheight - imginfo->bmar - imginfo->tmar;
    }
    if (maxwidth == UNDEF && maxheight == UNDEF) {
      maxwidth = max(info.paperwidth - 144, info.paperwidth * 0.75);
      maxheight = max(info.paperheight - 144, info.paperheight * 0.75);
    }
    
    if (maxwidth == UNDEF) {
      sc = maxheight / imginfo->trans.bb[1];
    } else if (maxheight == UNDEF) {
      sc = maxwidth / imginfo->trans.bb[0];
    } else {
      sc = min(maxwidth / imginfo->trans.bb[0], maxheight / imginfo->trans.bb[1]);
    }

    /* re-scale coordinate system */
    imginfo->width *= sc;
    imginfo->height *= sc;
    trans_rescale(&imginfo->trans, sc);
  }

  /* adjust margins */
  if (info.backend->fixed) {
    if (imginfo->lmar == UNDEF && imginfo->rmar == UNDEF) {
      imginfo->lmar = (info.paperwidth-imginfo->trans.bb[0])/2;
    } else if (imginfo->lmar == UNDEF) {
      imginfo->lmar = (info.paperwidth-imginfo->trans.bb[0]-imginfo->rmar);
    } else if (imginfo->lmar != UNDEF && imginfo->rmar != UNDEF) {
      imginfo->lmar += (info.paperwidth-imginfo->trans.bb[0]-imginfo->lmar-imginfo->rmar)/2;
    }
    if (imginfo->bmar == UNDEF && imginfo->tmar == UNDEF) {
      imginfo->bmar = (info.paperheight-imginfo->trans.bb[1])/2;
    } else if (imginfo->bmar == UNDEF) {
      imginfo->bmar = (info.paperheight-imginfo->trans.bb[1]-imginfo->tmar);
    } else if (imginfo->bmar != UNDEF && imginfo->tmar != UNDEF) {
      imginfo->bmar += (info.paperheight-imginfo->trans.bb[1]-imginfo->bmar-imginfo->tmar)/2;
    }
  } else {
    if (imginfo->lmar == UNDEF) {
      imginfo->lmar = 0;
    }
    if (imginfo->rmar == UNDEF) {
      imginfo->rmar = 0;
    }
    if (imginfo->bmar == UNDEF) {
      imginfo->bmar = 0;
    }
    if (imginfo->tmar == UNDEF) {
      imginfo->tmar = 0;
    }
  }
}

/* ---------------------------------------------------------------------- */
/* auxiliary functions for file handling */

/* open a file for reading. Return stdin if filename is NULL or "-" */ 
static FILE *my_fopen_read(const char *filename) {
  if (filename == NULL || strcmp(filename, "-") == 0) {
    return stdin;
  }
  return fopen(filename, "rb");
}

/* open a file for writing. Return stdout if filename is NULL or "-" */ 
static FILE *my_fopen_write(const char *filename) {
  if (filename == NULL || strcmp(filename, "-") == 0) {
    return stdout;
  }
  return fopen(filename, "wb");
}

/* close a file, but do nothing is filename is NULL or "-" */
static void my_fclose(FILE *f, const char *filename) {
  if (filename == NULL || strcmp(filename, "-") == 0) {
    return;
  }
  fclose(f);
}

static int fill_bm(unsigned char* data, int w, int h, potrace_bitmap_t** bmp) {
  potrace_bitmap_t* bm;

  /* allocate bitmap */
  bm = bm_new(w, h);
  if (!bm) {
    goto std_error;
  }

  for(int y=0; y<h; y++) {
    for(int x=0; x<w; x++) {
        BM_UPUT(bm, x, y, data[y*w + x] > 0 ? 1 : 0);
    }
  }

  *bmp = bm;
  return 0;

  std_error:
    bm_free(bm);
    return -1;
}

static void process_data(backend_t *b, unsigned char* indata, int inw, int inh, char *sout, unsigned int outMaxLen) { 
  potrace_bitmap_t *bm = NULL; 
  imginfo_t imginfo;
  int eof_flag = 0;  /* to indicate premature eof */
  int count;         /* number of bitmaps successfully processed, this file */
  potrace_state_t *st;

  for (count=0; ; count++) {
    /* read a bitmap */
    int r = fill_bm(indata, inw, inh, &bm);
    if(r < 0) {
      fprintf(stderr, "" POTRACE ": %s\n", strerror(errno));
      exit(2);
    }

    /* process the image */
    st = potrace_trace(info.param, bm);
    if (!st || st->status != POTRACE_STATUS_OK) {
      fprintf(stderr, "" POTRACE ": %s\n", strerror(errno));
      exit(2);
    }

    /* calculate image dimensions */
    imginfo.pixwidth = bm->w;
    imginfo.pixheight = bm->h;
    bm_free(bm);

    calc_dimensions(&imginfo, st->plist);

    r = b->page_f(sout, outMaxLen, st->plist, &imginfo);
    if (r) {
      fprintf(stderr, "" POTRACE ": %s\n", strerror(errno));
      exit(2);
    }

    potrace_state_free(st);

    if (eof_flag || !b->multi) {
      return;
    }
  }
  /* not reached */
}

#include "curve.h"

void process_data_onlypolygon(unsigned char* indata, int inw, int inh, long* sout, unsigned int outMaxLen) { 
  int r; 
  potrace_bitmap_t *bm = NULL; 
  potrace_state_t *st;

  /* read a bitmap */
  r = fill_bm(indata, inw, inh, &bm);
  if(r < 0) {
    fprintf(stderr, "" POTRACE ": %s\n", strerror(errno));
    exit(2);
  }

  /* process the image */
  st = potrace_trace_onlypolygon(info.param, bm);
  if (!st || st->status != POTRACE_STATUS_OK) {
    fprintf(stderr, "" POTRACE ": %s\n", strerror(errno));
    exit(2);
  }

  /* Insert paths into outarray */
  potrace_path_t* nowPath = st->plist;
  unsigned int oi = 0;
  while(nowPath) {
    potrace_privpath_t* pp = nowPath->priv;
    int len = pp->m;
    int* indices = pp->po;
    point_t* points = pp->pt;

    // Insert path into outarray
    for(int i=0; i<len; i++) {
      if(oi+1 >= outMaxLen) {
        fprintf(stderr, "" POTRACE ": Too small output buffer, we need %u\n", oi+2);
        exit(2);
      }

      point_t* point = points + indices[i];
      sout[oi] = point->x;
      sout[oi+1] = point->y;
      oi += 2;
    }

    // Insert SEP(inw+1)
    {
      if(oi >= outMaxLen) {
        fprintf(stderr, "" POTRACE ": Too small output buffer, we need %u\n", oi+1);
        exit(2);
      }
      sout[oi] = inw + 1;
      oi++;
    }

    nowPath = nowPath->next;
  }

  // Insert END(inw+1)
  {
    if(oi >= outMaxLen) {
      fprintf(stderr, "" POTRACE ": Too small output buffer, we need %u\n", oi+1);
      exit(2);
    }
    sout[oi] = inw + 1;
    oi++;
  }

  potrace_state_free(st);
}

/* ---------------------------------------------------------------------- */
/* Port functions */

#define TRY(x) if (x) goto try_error

/*
Trace a bitmap.
params--
  inarray: an array with the size of inw x inh, containing a bitmap's alpha channel, whose value is ranged in [0, 255]
      e.g. image[y][x].a == inarray[y*inw + inh]
      Note we assume the y-axis is up.
      y
      /\
      |
      |
      |
    inh
      |
      |
      |
      ------------inw------------>x
  outarray: an array with the size of outMaxLen, which will contain svg string of ASCII coding.
      If the svg's length is longer than outMaxLen, error msg will be printed, and the program will exit with code 2.

return--
  If succeed, return 0. Otherwise, print the error msg, and return a non-zero value.
*/
int trace(unsigned char *inarray, int inw, int inh, char* outarray, unsigned int outMaxLen) {
  backend_t *b;  /* backend info */

  /* process options */
  dopts();

  b = info.backend;
  if (b==NULL) {
    fprintf(stderr, "" POTRACE ": internal error: selected backend not found\n");
    return 1;
  }

  /* fix some parameters */
  /* if backend cannot handle opticurve, disable it */
  if (b->opticurve == 0) {
    info.param->opticurve = 0;
  }

  process_data(b, inarray, inw, inh, outarray, outMaxLen);

  potrace_param_free(info.param);
  return 0;

  /* not reached*/

 try_error:
  fprintf(stderr, "" POTRACE ": %s\n", strerror(errno));
  return 2;
}


/*
Trace a bitmap, outputing a optimal polygon
params--
  inarray: an array with the size of inw x inh, containing a bitmap's alpha channel, whose value is ranged in [0, 255]
      e.g. image[y][x].a == inarray[y*inw + inh]
      Note we assume the y-axis is up.
      y
      /\
      |
      |
      |
    inh
      |
      |
      |
      ------------inw------------>x
  outarray: an array of polygons: {p0, SEP, p1, SEP, ..., pn, END}
      SEP = inw+1, END = inw+1
      pi is a polygon: v0, v1, v2, ..., vm
      vj is a point: x, y
      point is not the pixel. point is the abstract position between four near pixels.
        e.g.
        PIXEL       PIXEL
              point      
        PIXEL       PIXEL
      point's x,y is in the range of [0, inw], [0, inh]

return--
  If succeed, return 0. Otherwise, print the error msg, and return a non-zero value.
*/
int trace_onlypolygon(unsigned char *inarray, int inw, int inh, long* outarray, unsigned int outMaxLen) {
  /* process options */
  dopts();

  process_data_onlypolygon(inarray, inw, inh, outarray, outMaxLen);

  potrace_param_free(info.param);
  return 0;

  /* not reached*/

 try_error:
  fprintf(stderr, "" POTRACE ": %s\n", strerror(errno));
  return 2;
}
