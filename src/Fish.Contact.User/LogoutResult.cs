﻿using IdentityServer4.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fish.Contact.User
{
    public class LogoutResult : LogoutRequest
    {
        public LogoutResult(string iframeUrl, LogoutMessage message) : base(iframeUrl, message)
        {
        }
    }
}
