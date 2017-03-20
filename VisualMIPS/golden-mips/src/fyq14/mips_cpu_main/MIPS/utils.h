#ifndef MIPS_UTILS_H
#define MIPS_UTILS_H

#include "mips_core.h"

// ================================
// Utils' headers
// ================================
// FinalAction Class, exploting RAII to emulate a Java "finally" block
// reference/source: http://stackoverflow.com/a/25510879/3927334
namespace MIPS {
    template <typename F>
    class FinalAction {
    protected:
        F clean;
        bool enabled;
    public:
        FinalAction(F);
        ~FinalAction();
        void disable();
    };
}

// why macro over function (taht returns a Finalaction Object)? 
// => Not sure if the copy constructor
//      will be invoked by certain compilers (i.e, the clean up functiom
//      will be called when the function returns the FinalAction object)

#define FINALLY(f) MIPS::FinalAction<decltype(f)>(f)


// ================================
// Definitions
// ================================

namespace MIPS {
    template<typename F>
    FinalAction<F>::FinalAction(F f_in) :
        clean(f_in), 
        enabled(true) {}

    template<typename F>
    FinalAction<F>::~FinalAction() {
        if (enabled) {
            clean();
        }
    }

    template<typename F>
    void FinalAction<F>::disable() {
        enabled = false;
    }
}


#endif
