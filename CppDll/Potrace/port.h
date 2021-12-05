#ifndef PORT_H
#define PORT_H

int trace(unsigned char *inarray, int inw, int inh, char* outarray, unsigned int outMaxLen);
int trace_onlypolygon(unsigned char *inarray, int inw, int inh, long* outarray, unsigned int outMaxLen);

#endif// PORT_H