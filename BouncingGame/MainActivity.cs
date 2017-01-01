using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Util;

using CocosSharp;

namespace BouncingGame
{
    [Activity(Label = "BouncingGame", MainLauncher = true, Icon = "@drawable/icon",
        ScreenOrientation = ScreenOrientation.Landscape,
        AlwaysRetainTaskState = true,
        LaunchMode = LaunchMode.SingleInstance,
        Theme = "@android:style/Theme.Black.NoTitleBar.Fullscreen",
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden)]
    public class MainActivity : Activity
    {
        int count = 1;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our game view from the layout resource,
            // and attach the view created event to it
            CCGameView gameView = (CCGameView)FindViewById(Resource.Id.GameView);
            gameView.ViewCreated += LoadGame;
        }

        CCGameView _gameView = null;

        CCScene _gameScene = null;
        CCScene _menuScene = null;
        GameLayer _gameLayer = null;


        public void RunGame(EasingFactory factory)
        {
            if (_gameScene == null)
            {
                _gameScene = new CCScene(_gameView);
                _gameLayer = new GameLayer(this);
                _gameScene.AddLayer(_gameLayer);
                _gameScene.AddLayer(new MenuLayer2(this));
            }
            _gameLayer.SetFactory(factory);
            _gameLayer.StartGame();
            _gameView.RunWithScene(_gameScene);
        }

        public void RunMenu()
        {
            if (_menuScene == null)
            {
                _menuScene = new CCScene(_gameView);
                _menuScene.AddLayer(new MenuLayer(this));
            }
            _gameView.RunWithScene(_menuScene);
        }

        void PlayBackgroundMusic()
        {
            try
            {
                CCAudioEngine.SharedEngine.BackgroundMusicVolume = 0.08f;
                CCAudioEngine.SharedEngine.EffectsVolume = 1f;
                CCAudioEngine.SharedEngine.PlayBackgroundMusic(filename: "background_theme", loop: true);
            }
            catch (Exception ex)
            {
                Log.Info("adyga_pazzle", "[ERROR] can't play background music {0}", ex);
            }
        }

        void LoadGame(object sender, EventArgs e)
        {
            CCGameView gameView = sender as CCGameView;

            if (gameView != null)
            {
                var contentSearchPaths = new List<string>() { "Fonts", "Sounds", "Images/Animals" };
                CCSizeI viewSize = gameView.ViewSize;

                int width = 960;
                int height = 540;

                // Set world dimensions
                gameView.DesignResolution = new CCSizeI(width, height);
                gameView.ResolutionPolicy = CCViewResolutionPolicy.ShowAll;

                // Determine whether to use the high or low def versions of our images
                // Make sure the default texel to content size ratio is set correctly
                // Of course you're free to have a finer set of image resolutions e.g (ld, hd, super-hd)
                if (width < viewSize.Width)
                {
                    contentSearchPaths.Add("Images/Hd");
                    //CCSprite.DefaultTexelToContentSizeRatio = 2.0f;
                }
                else
                {
                    contentSearchPaths.Add("Images/Ld");
                    CCSprite.DefaultTexelToContentSizeRatio = 1.0f;
                }

                gameView.ContentManager.SearchPaths = contentSearchPaths;
                PlayBackgroundMusic();

                // Construct game scene
                _gameView = gameView;
                RunMenu();
            }
        }
    }
}

