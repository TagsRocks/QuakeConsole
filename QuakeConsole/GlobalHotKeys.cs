﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace QuakeConsole
{
    enum Modifiers 
    {
        Alt = 0x0001,
        Ctrl = 0x0002,
        NoRepeat = 0x4000,  
        Shift = 0x0004,
        Win = 0x0008 
    }

    class GlobalHotKey
    {
        public static int WM_HOTKEY = 0x312;
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private IntPtr handle;
        public int id;
        public Keys keys;
        public Modifiers modifiers;
        public Action handler;
        
        public GlobalHotKey(IntPtr handle, Keys keys, Modifiers modifiers, Action handler)
        {
            this.handle = handle;
            this.keys = keys;
            this.modifiers = modifiers;
            this.handler = handler;
            this.id = (int)keys ^ (int)modifiers;
        }

        public void Call() 
        {
            this.handler();
        }

        public void Enable()
        {
            RegisterHotKey(handle, id, (uint)modifiers, (uint)keys);
        }

        public void Disable()
        {
            UnregisterHotKey(handle, id);
        }
    }

    class GlobalHotKeys : NativeWindow, IDisposable
    {
        private List<GlobalHotKey> hotkeys;
        public GlobalHotKeys()
        {
            hotkeys = new List<GlobalHotKey>();
            CreateHandle(new CreateParams());
        }

        ~GlobalHotKeys()
        {
            Dispose();
        }

        public void Dispose()
        {
            hotkeys.ForEach(hotkey => hotkey.Disable());
            this.DestroyHandle();
        }

        public GlobalHotKey Register(Keys keys, Modifiers modifiers, Action handler)
        {
            GlobalHotKey hotkey = new GlobalHotKey(this.Handle, keys, modifiers, handler);
            hotkeys.Add(hotkey);
            return hotkey;
        }

        public GlobalHotKey Register(Keys keys, Modifiers modifiers, Action handler, bool enabled)
        {
            var hotkey = Register(keys, modifiers, handler);
            hotkey.Enable();
            return hotkey;
        }


        protected override void WndProc(ref Message message)
        {
            base.WndProc(ref message);
            if (message.Msg == GlobalHotKey.WM_HOTKEY)
            {
                int id = message.WParam.ToInt32();
                hotkeys.Find( item => item.id == id ).Call();
            }
        }
    }
}
