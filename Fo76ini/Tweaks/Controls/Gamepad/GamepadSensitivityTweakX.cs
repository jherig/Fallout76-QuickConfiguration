﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fo76ini.Tweaks.Controls
{
    class GamepadSensitivityTweakX : ITweak<float>, ITweakInfo
    {
        public string Description => "";

        public WarnLevel WarnLevel => WarnLevel.None;

        public string AffectedFiles => "Fallout76Prefs.ini";

        public string AffectedValues => "[Controls]fGamepadHeadingSensitivity";

        public float DefaultValue => 0.6667f;

        public string Identifier => this.GetType().FullName;

        public bool UIReloadNecessary => false;

        public float GetValue()
        {
            return IniFiles.GetFloat("Controls", "fGamepadHeadingSensitivity", DefaultValue);
        }

        public void SetValue(float value)
        {
            IniFiles.F76Prefs.Set("Controls", "fGamepadHeadingSensitivity", value);
        }

        public void ResetValue()
        {
            SetValue(DefaultValue);
        }
    }
}
