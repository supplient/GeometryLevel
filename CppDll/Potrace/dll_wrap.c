#include "port.h"
#include <stdint.h>

__declspec(dllexport) int __stdcall trace_onlypolygon_wrap(unsigned char* inarray, int inw, int inh, int32_t* outarray, uint32_t outMaxLen) {
    return trace_onlypolygon(inarray, inw, inh, outarray, outMaxLen);
}
