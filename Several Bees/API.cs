using System.Collections.Generic;
using UnityEngine;
using System;
using Random = System.Random;

namespace SeveralBees
{
    public class Api : MonoBehaviour
    {
        public static Api Instance { get; private set; }

        private void Awake()
        {
            UnityEngine.Debug.Log("[Several Bees] Api Awake");
            Instance = this;
            UnityEngine.Debug.Log("[Several Bees] Api Instance Set");
        }

        /*-------------------------------------------------------------------------------------------------------------------*/








        internal Dictionary<string, string> tokenList = new Dictionary<string, string>();
        internal Dictionary<string, bool> tokenListVisable = new Dictionary<string, bool>();
        internal Dictionary<string, string> tokenListBackToken = new Dictionary<string, string>();
        internal Dictionary<string, List<ModButtonInfo>> tokenListButtonInfo = new Dictionary<string, List<ModButtonInfo>>();
        private Random random = new Random();

        internal bool HasMadeSettings = false;
        public string GenerateToken(string Name, bool Visable = true, string Back = "Main")
        {
            if (!HasMadeSettings)
            {
                SeveralBees.Instance.MakeSettings();
                HasMadeSettings = true;
            }
            string token;

            do
            {
                token = GenerateRandomToken();
            } while (tokenList.ContainsValue(token));

            tokenList[Name] = token;
            tokenListVisable[token] = Visable;
            tokenListBackToken[token] = Back;
            tokenListButtonInfo[token] = new List<ModButtonInfo>();
            return token;
        }
        private string GenerateRandomToken()
        {
            char[] digits = new char[15];
            for (int i = 0; i < 15; i++)
            {
                digits[i] = (char)('0' + random.Next(0, 10));
            }
            return new string(digits);
        }

        public List<ModButtonInfo> GetButtonInfo(string token)
        {
            if (tokenListButtonInfo.TryGetValue(token, out List<ModButtonInfo> buttonInfo))
            {
                return buttonInfo;
            }
            return null;
        }

        public void SetButtonInfo(string token, List<ModButtonInfo> buttonInfo)
        {
            if (tokenListButtonInfo.ContainsKey(token))
            {
                tokenListButtonInfo[token] = buttonInfo;
            }
        }

        public void OpenMenu(string token)
        {
            SeveralBees.Instance.SectionName = token;
            SeveralBees.Instance.PointerPositionIndex = 0;
        }

        public ModButtonInfo GrabButton(string token, string name)
        {
            List<ModButtonInfo> btninfo = GetButtonInfo(token);
            foreach (ModButtonInfo info in btninfo)
            {
                if (info.buttonText == name)
                {
                    return info;
                }
            }
            return null;
        }

        public void SetButtonInfoSignle(string token, string name, ModButtonInfo NewButtonInfo)
        {
            List<ModButtonInfo> btninfo = GetButtonInfo(token);
            btninfo[btninfo.FindIndex(info => info.buttonText == name)] = NewButtonInfo;
        }

        public void SetButtonNickname(string token, string name, string Nickname)
        {
            List<ModButtonInfo> btninfo = GetButtonInfo(token);
            btninfo[btninfo.FindIndex(info => info.buttonText == name)].buttonOverlayText = Nickname;
        }
    }

    public class ModButtonInfo
    {
        public string buttonText;
        public string buttonOverlayText = null;
        public bool isTogglable;
        public bool enabled;
        public Action method;
        public Action enableMethod;
        public Action disableMethod;
        public Action enableUpdate;
        public Action disableUpdate;
        public string toolTip = null;
    }
}
