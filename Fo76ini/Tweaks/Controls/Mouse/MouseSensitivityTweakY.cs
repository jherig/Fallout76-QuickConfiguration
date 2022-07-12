﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fo76ini.Tweaks.Controls
{
    class MouseSensitivityTweakY : ITweak<float>, ITweakInfo
    {
        public string Description => "";

        public WarnLevel WarnLevel => WarnLevel.None;

        public string AffectedFiles => "Fallout76Prefs.ini";

        public string AffectedValues => "[Controls]fMouseHeadingSensitivityY";

        public float DefaultValue => 0.03f;

        public string Identifier => this.GetType().FullName;

        public float GetValue()
        {
            return IniFiles.GetFloat("Controls", "fMouseHeadingSensitivityY", DefaultValue);
        }

        public void SetValue(float value)
        {
            IniFiles.F76Prefs.Set("Controls", "fMouseHeadingSensitivityY", value);
        }

        public void ResetValue()
        {
            SetValue(DefaultValue);
        }
    }
}
