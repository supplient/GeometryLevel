/* Copyright (C) 2001-2019 Peter Selinger.
   This file is part of Potrace. It is free software and it is covered
   by the GNU General Public License. See the file COPYING for details. */

#include <stdio.h>
#include <stdlib.h>

#include "port.h"


/* ---------------------------------------------------------------------- */
/* main: handle file i/o */

int main(int ac, char *av[]) {

  // Prepare input data
  int inw = 8;
  int inh = 8;
  const unsigned char inarrayConst[8*8] = {
      /*
  // 0    1    2    3    4    5    6    7    8
      000, 000, 000, 000, 000, 000, 000, 000,
  // 1
      000, 000, 255, 255, 000, 000, 000, 000,
  // 2
      000, 255, 255, 255, 255, 000, 000, 000,
  // 3
      000, 255, 000, 000, 255, 255, 255, 000,
  // 4
      000, 255, 000, 000, 000, 255, 255, 000,
  // 5
      000, 255, 255, 000, 000, 000, 255, 000,
  // 6
      000, 255, 255, 255, 255, 255, 255, 000,
  // 7
      000, 000, 000, 000, 000, 000, 255, 000,
  // 8
      */
    //  000, 000, 000, 000, 000, 000, 000, 000, 
    //  000, 255, 255, 255, 255, 255, 255, 000, 
    //  000, 000, 255, 255, 000, 000, 255, 000, 
    //  000, 000, 000, 000, 255, 255, 255, 000, 
    //  000, 000, 000, 000, 000, 000, 255, 000, 
    //  000, 000, 000, 000, 000, 000, 000, 000, 
    //  000, 000, 000, 000, 000, 000, 000, 000, 
    //  000, 000, 000, 000, 000, 000, 000, 000, 
		0, 0, 0, 0, 0, 0, 0, 0,
		0, 1, 1, 1, 1, 1, 1, 0,
		0, 1, 0, 0, 0, 0, 1, 0,
		0, 1, 0, 0, 0, 0, 1, 0,
		0, 1, 0, 0, 0, 0, 1, 0,
		0, 1, 0, 0, 0, 0, 1, 0,
		0, 1, 1, 1, 1, 1, 1, 0,
		0, 0, 0, 0, 0, 0, 0, 0,
  };

  unsigned char *inarray = malloc(sizeof(int) * inw * inh);
  for(int y=0; y<inh; y++) {
    for(int x=0; x<inw; x++) {
      inarray[y*inw + x] = inarrayConst[y*inw + x];
    }
  }

  // Alloc memory for output
  long *outarray = malloc(sizeof(long) * 10000);

  // Trace
  int r = trace_onlypolygon(inarray, inw, inh, outarray, 10000);
  if(r)
    exit(r);

  // Output the result
  unsigned int oi = 0;
  while(outarray[oi] != inw+1) {
    while(outarray[oi] != inw+1) {
      fprintf(stdout, "(%ld, %ld) ", outarray[oi], outarray[oi+1]);
      oi += 2;
    }
    oi++;
    fprintf(stdout, "\n");
  }

  // Free memory
  free(inarray);
  free(outarray);
  return 0;
}
