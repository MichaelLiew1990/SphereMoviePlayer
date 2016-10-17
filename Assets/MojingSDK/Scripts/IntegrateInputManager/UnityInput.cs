using System.Collections.Generic;
using UnityEngine;
using MojingSample.CrossPlatformInput;

namespace MojingSample.CrossPlatformInput.UnityInput
{
public partial class UnityInput : MonoBehaviour,InputSource
    {

        [System.Serializable]
        public class EntryString
        {
            public string key, value;

            public EntryString() : this("Null")
            {
            }

            public EntryString(string i_key)
            {
                key = value = i_key;
            }
        }

        public bool mapMouse = false;

        public EntryString[] 
            axes = new EntryString[4]{
			new EntryString("Horizontal"),
            new EntryString("Vertical"),
			new EntryString("Mouse X"),
            new EntryString("Mouse Y")
		},
            buttons = new EntryString[10]{
			new EntryString("Fire1"),//left ctrl
            new EntryString("Fire2"),//left alt
			new EntryString("Fire3"),//left cmd
            new EntryString("Jump"),//space
            new EntryString("Cancel"),//Esc
            new EntryString("Submit"),//Enter
            new EntryString("LEFT"),//left
			new EntryString("RIGHT"),//right
            new EntryString("UP"),//up
            new EntryString("DOWN")//down
		}
        ;
        public int numAxes = -1, numButtons = -1;

        public Dictionary<string, EntryString> mapInput;

        protected CrossPlatformInputManager.VirtualAxis[] _aHandles;
        protected CrossPlatformInputManager.VirtualButton[] _bHandles;


        public virtual int InitInput()
        {
            // Check number of virtual elements.
            if (numAxes < 0)
                numAxes = axes.Length;
            if (numButtons < 0)
                numButtons = buttons.Length;
            // Register virtual elements.
            mapInput = new Dictionary<string, EntryString>(numAxes + numButtons);
            int i;
            // VirtualAxis
            _aHandles = new CrossPlatformInputManager.VirtualAxis[numAxes];
            for (i = 0; i < numAxes; ++i)
            {
				CrossPlatformInputManager.VirtualAxis va = null;

                //va = CrossPlatformInputManager.VirtualAxisReference(axes[i].key);
			va = CrossPlatformInputManager.VirtualAxisReference(this, axes[i].key, true);
				_aHandles[i] = va;
                mapInput.Add(axes[i].key, axes[i]);
            }
            // VirtualButton
            _bHandles = new CrossPlatformInputManager.VirtualButton[numButtons];
            for (i = 0; i < numButtons; ++i)
            {
                _bHandles[i] = CrossPlatformInputManager.VirtualButtonReference(this,buttons[i].key,true);
                mapInput.Add(buttons[i].key, buttons[i]);
            }
            return 0;
        }

        public int ExitInput()
        {
            return 0;
        }

        public int EnterInputFrame()
        {
            if (mapMouse)
            {
                Vector3 vec = Input.mousePosition;
                CrossPlatformInputManager.SetVirtualMousePositionX(vec.x);
                CrossPlatformInputManager.SetVirtualMousePositionY(vec.y);
                CrossPlatformInputManager.SetVirtualMousePositionZ(vec.z);
            }

//Only enable in Editor Mode
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            int i;
            for (i = 0; i < numAxes; ++i)
            {
                //CrossPlatformInputManager.VirtualAxis va = _aHandles[i];
                _aHandles[i].Update(Input.GetAxisRaw(axes[i].value));
            }
            for (i = 0; i < numButtons; ++i)
            {

                if (Input.GetButtonDown(buttons[i].value))
                {
                    _bHandles[i].Pressed();
                }
                else if (Input.GetButtonUp(buttons[i].value))
                {
                    _bHandles[i].Released();
                }
            }
#endif
            return 0;
        }

        public int ExitInputFrame()
        {
            return 0;
        }

    }
}