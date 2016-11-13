using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Input;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using Windows.Storage.Streams;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace EmojiDial
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        RadialController dial;
        RadialControllerConfiguration dialConfig;
        List<RadialControllerMenuItem> menuItems;
        int selectedItem = 0;
        bool isFocused;

        public MainPage()
        {
            this.InitializeComponent();
            dial = RadialController.CreateForCurrentView();
            dial.RotationResolutionInDegrees = 10;
            dialConfig = RadialControllerConfiguration.GetForCurrentView();
            menuItems = new List<RadialControllerMenuItem>();
            isFocused = true;
            dial.ButtonClicked += Dial_ButtonClicked;
            dial.RotationChanged += Dial_RotationChanged;
            dial.ControlAcquired += Dial_ControlAcquired;
            dial.ControlLost += Dial_ControlLost;
            BuildMainMenu();
        }

        string UnicodeToString(int codePoint)
        {
            // subtract 0x10000
            codePoint -= 0x10000;

            // get the first 10
            char first10 = (char)(codePoint >> 10);

            // get the last 10
            char last10 = (char)(codePoint & 0x3FF);

            // create the first char
            first10 += (char)0xD800;

            // create the last char
            last10 += (char)0xDC00;

            // return the string
            return new string(new char[] { first10, last10 });
        }

        private void Dial_ControlLost(RadialController sender, object args)
        {
            isFocused = false;
        }

        private void Dial_ControlAcquired(RadialController sender, RadialControllerControlAcquiredEventArgs args)
        {
            isFocused = true;
        }

        private void Dial_ButtonClicked(RadialController sender, RadialControllerButtonClickedEventArgs args)
        {
            var item = dial.Menu.GetSelectedMenuItem();
            if (item.DisplayText == "Back")
            {
                GoBack();
            }
            else if (item.DisplayText == "Smileys")
            {
                BuildSmileys();
            }
            else
            {
                PrintEmoji(item.DisplayText);
            }
        }

        private void Dial_RotationChanged(RadialController sender, RadialControllerRotationChangedEventArgs args)
        {
            if (args.RotationDeltaInDegrees > 0)
            {
                selectedItem++;
            }
            else if (args.RotationDeltaInDegrees < 0)
            {
                selectedItem--;
            }
            if (selectedItem < 0)
            {
                selectedItem = menuItems.Count - 1;
            }
            else if (selectedItem >= menuItems.Count)
            {
                selectedItem = 0;
            }
            var item = menuItems[selectedItem];
            dial.Menu.SelectMenuItem(item);
            FadeOut(item.DisplayText);
        }

        async Task FadeOut(string text)
        {
            TextBlock testBlock = new TextBlock();
            testBlock.HorizontalAlignment = HorizontalAlignment.Left;
            testBlock.VerticalAlignment = VerticalAlignment.Bottom;
            var thickness = new Thickness(32, 0, 0, 32);
            testBlock.Margin = thickness;
            testBlock.FontSize = 48;
            testBlock.Text = text;
            grid.Children.Add(testBlock);
            while (testBlock.Opacity > 0)
            {
                testBlock.Opacity -= 0.1;
                thickness.Bottom += 0.5;
                testBlock.Margin = thickness;
                await Task.Delay(TimeSpan.FromMilliseconds(50));
            }
            grid.Children.Remove(testBlock);
        }

        public void BuildMainMenu()
        {
            ClearDialItems();

            var recent = RadialControllerMenuItem.CreateFromIcon("Recent", RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/recent.png")));
            recent.Invoked += Recent_Invoked;
            menuItems.Add(recent);

            var smilies = RadialControllerMenuItem.CreateFromIcon("Smileys", RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/emoticons.png")));
            smilies.Invoked += Smileys_Invoked;
            menuItems.Add(smilies);

            var people = RadialControllerMenuItem.CreateFromIcon("People", RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/people.png")));
            people.Invoked += People_Invoked;
            menuItems.Add(people);

            var objects = RadialControllerMenuItem.CreateFromIcon("Objects", RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/objects.png")));
            objects.Invoked += Objects_Invoked;
            menuItems.Add(objects);

            var food = RadialControllerMenuItem.CreateFromIcon("Food", RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/food.png")));
            food.Invoked += Food_Invoked;
            menuItems.Add(food);

            var places = RadialControllerMenuItem.CreateFromIcon("Travel & Places", RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/travel.png")));
            places.Invoked += Places_Invoked;
            menuItems.Add(places);

            var symbols = RadialControllerMenuItem.CreateFromIcon("Symbols", RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/symbols.png")));
            symbols.Invoked += Symbols_Invoked;
            menuItems.Add(symbols);

            BuildDialMenu();
        }

        private void Recent_Invoked(RadialControllerMenuItem sender, object args)
        {
            if (!isFocused)
            {
                AddBackButtons();
            }
        }

        private void Smileys_Invoked(RadialControllerMenuItem sender, object args)
        {
            if (!isFocused)
            {
                BuildSmileys();
            }
        }

        void BuildSmileys()
        {
            ClearDialItems();
            AddBackButtons();
            AddSmileys();
            BuildDialMenu();
        }

        public void AddSmileys()
        {
            // emoticons
            for (int i = 0x1F600; i < 0x1F650; i++)
            {
                if (i == 0x1F600)
                {
                    CreateEmoji(UnicodeToString(i), "http://emojipedia-us.s3.amazonaws.com/cache/09/7b/097bc43e98693ef7cb037aa86ad1b41d.png");
                }
                else if (i == 0x1F601)
                {
                    CreateEmoji(UnicodeToString(i), "http://emojipedia-us.s3.amazonaws.com/cache/4b/a7/4ba715bac5a2bf6e17cc9da808064de7.png");
                }
                else if (i == 0x1F602)
                {
                    CreateEmoji(UnicodeToString(i), "http://emojipedia-us.s3.amazonaws.com/cache/78/65/78659af99f7876577c30ba6beae4b3a5.png");
                }
                else if (i == 0x1F603)
                {
                    CreateEmoji(UnicodeToString(i), "http://emojipedia-us.s3.amazonaws.com/cache/d4/6c/d46cb6afa419818df1319a63d88428aa.png");
                }
                else if (i == 0x1F604)
                {
                    CreateEmoji(UnicodeToString(i), "http://emojipedia-us.s3.amazonaws.com/cache/ae/5b/ae5b2de248425a392f0ed40ac645d7fe.png");
                }
                else if (i == 0x1F605)
                {
                    CreateEmoji(UnicodeToString(i), "http://emojipedia-us.s3.amazonaws.com/cache/b8/d6/b8d64eef78956a24fd4035fe31be7d3a.png");
                }
                else if (i == 0x1F606)
                {
                    CreateEmoji(UnicodeToString(i), "http://emojipedia-us.s3.amazonaws.com/cache/15/d4/15d464ee1a91e67b0033bb750d0f93ca.png");
                }
                else if (i == 0x1F607)
                {
                    CreateEmoji(UnicodeToString(i), "http://emojipedia-us.s3.amazonaws.com/cache/d3/4d/d34d18d4f9a006f95e90f3e1d6713b1e.png");
                }
                else if (i == 0x1F608)
                {
                    CreateEmoji(UnicodeToString(i), "http://emojipedia-us.s3.amazonaws.com/cache/46/34/4634939b1f7dfd69eec7163e10ba4082.png");
                }
                else if (i == 0x1F609)
                {
                    CreateEmoji(UnicodeToString(i), "http://emojipedia-us.s3.amazonaws.com/cache/7a/62/7a628f895b658ce4fac89702a49b5d2a.png");
                }
                else
                {
                    CreateEmoji(UnicodeToString(i));
                }
            }
        }

        void CreateEmoji(string emoji)
        {
            var emojiItem = RadialControllerMenuItem.CreateFromKnownIcon(emoji, RadialControllerMenuKnownIcon.InkColor);
            int index = menuItems.Count;
            emojiItem.Invoked += (s, a) =>
            {
                selectedItem = index;
            };
            menuItems.Add(emojiItem);
        }

        void CreateEmoji(string emoji, string url)
        {
            var emojiItem = RadialControllerMenuItem.CreateFromIcon(emoji, RandomAccessStreamReference.CreateFromUri(new Uri(url)));
            int index = menuItems.Count;
            emojiItem.Invoked += (s, a) =>
            {
                selectedItem = index;
            };
            menuItems.Add(emojiItem);
        }

        public void PrintEmoji(string emoji)
        {
            emojiBox.Text += emoji;
        }

        private void People_Invoked(RadialControllerMenuItem sender, object args)
        {
            if (!isFocused)
            {
                AddBackButtons();
            }
        }

        private void Objects_Invoked(RadialControllerMenuItem sender, object args)
        {
            if (!isFocused)
            {
                AddBackButtons();
            }
        }

        private void Food_Invoked(RadialControllerMenuItem sender, object args)
        {
            if (!isFocused)
            {
                AddBackButtons();
            }
        }

        private void Places_Invoked(RadialControllerMenuItem sender, object args)
        {
            if (!isFocused)
            {
                AddBackButtons();
            }
        }

        private void Symbols_Invoked(RadialControllerMenuItem sender, object args)
        {
            if (!isFocused)
            {
                AddBackButtons();
            }
        }

        public void BuildDialMenu()
        {
            foreach (var item in menuItems)
            {
                dial.Menu.Items.Add(item);
            }
            ClearSystemMenuItems();
        }

        public void ClearSystemMenuItems()
        {
            dialConfig.SetDefaultMenuItems(new List<RadialControllerSystemMenuItemKind>());
        }

        public void AddBackButtons()
        {
            var startBackButton = RadialControllerMenuItem.CreateFromKnownIcon("Back", RadialControllerMenuKnownIcon.UndoRedo);
            startBackButton.Invoked += (s, a) =>
            {
                if (!isFocused)
                {
                    GoBack();
                }
            };
            menuItems.Add(startBackButton);
        }

        public void GoBack()
        {
            BuildMainMenu();
        }

        public void ClearDialItems()
        {
            menuItems.Clear();
            dial.Menu.Items.Clear();
        }
    }
}
