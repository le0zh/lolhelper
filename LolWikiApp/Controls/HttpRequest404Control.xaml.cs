﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace LolWikiApp
{
    public partial class HttpRequest404Control : UserControl
    {
        public HttpRequest404Control()
        {
            InitializeComponent();

            DataContext = this;
        }

        public string Message { get; set; }
    }
}
