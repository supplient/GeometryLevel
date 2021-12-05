/* Copyright (C) 2001-2019 Peter Selinger.
   This file is part of Potrace. It is free software and it is covered
   by the GNU General Public License. See the file COPYING for details. */


#ifndef BACKEND_SVG_H
#define BACKEND_SVG_H

#include "potracelib.h"
#include "main.h"

int page_svg(char *sout, unsigned int outMaxLen, potrace_path_t *plist, imginfo_t *imginfo);

#endif /* BACKEND_SVG_H */

