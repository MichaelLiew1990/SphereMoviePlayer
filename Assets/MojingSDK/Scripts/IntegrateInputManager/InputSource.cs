using UnityEngine;
using System.Collections;

namespace MojingSample.CrossPlatformInput
{
    public interface InputSource
    {

        bool enabled
        {
            get;
            set;
        }

        int InitInput();

        int ExitInput();

        int EnterInputFrame();

        int ExitInputFrame();
    }
}