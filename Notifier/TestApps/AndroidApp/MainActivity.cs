﻿using Android.App;
using Android.OS;
using Android.Widget;
using Plugin.LocalNotifications;
using System;

namespace AndroidApp
{
    [Activity(Label = "AndroidApp", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        int count = 1;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);
            
            var testNotifierButton = FindViewById<Button>(Resource.Id.TestNotifierButton);

            testNotifierButton.Click += delegate
            {
                CrossLocalNotifications.Current.Show("Test", "This is a test notification");
                CrossLocalNotifications.Current.Show("Test", "This is a test notification from the future.", 1, DateTime.Now.AddMinutes(1), true, true);
            };

            var testAppLookupButton = FindViewById<Button>(Resource.Id.TestAppLookupButton);
        }
    }
}

