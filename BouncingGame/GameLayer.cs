using System;
using System.Collections.Generic;
using System.Linq;
using CocosSharp;
using Microsoft.Xna.Framework;
using Android.Util;
using System.IO;
using System.Timers;

namespace BouncingGame
{
    class Peace
    {
        public Peace(CCSprite s, CCPoint a)
        {
            Sprite = s;
            AssembledPos = a;
        }
        public CCSprite Sprite { get; private set; }
        public CCPoint DisassembledPos { get; set; }
        public CCPoint AssembledPos { get; private set; }
    }

    struct DraggingSpite
    {
        public DraggingSpite(Peace p, CCPoint s)
        {
            Peace = p;
            StartPosition = p.Sprite.Position;
            DragStart = s;
        }
        public Peace Peace;
        public CCPoint StartPosition;
        public CCPoint DragStart;
    }

    public delegate CCAction EasingFactory(CCFiniteTimeAction action);

    public class GameLayer : CCLayerColor
    {
        MainActivity _parent;
        image _currentImage;
        List<Peace> _peaces = new List<Peace>();
        DraggingSpite? _spiteToDrag = null;

        public GameLayer(MainActivity parent) : base(CCColor4B.Blue)
        {
            _parent = parent;
        }

        protected override void AddedToScene()
        {
            base.AddedToScene();

            // Use the bounds to layout the positioning of our drawable assets
            var bounds = VisibleBoundsWorldspace;
            // ----------------------- Start
            var assets = _parent.Assets;
            Log.Info("adyga_pazzle", "[XML_TEST] writing xml");
            // TODO: open with content manager not with android specific staff.
            using (var stackXml = assets.Open("Content/Images/Animals/Cat/stack.xml"))
            {
                _currentImage = ParseHelpers.ParseXML<image>(stackXml);
            }
            Log.Info("adyga_pazzle", "width = {0}, height={1} layers count={2}", _currentImage.w, _currentImage.h, _currentImage.stack.Length);
            Log.Info("adyga_pazzle", "VisibleBoundsWorldspace minX={0}, MinY={1}, maxX={2}, maxY={3}", bounds.MinX, bounds.MinY, bounds.MaxX, bounds.MaxY);
            Array.Reverse(_currentImage.stack);
            foreach (var peace in _currentImage.stack)
            {
                string prefix = "Cat";
                var spite = new CCSprite("Cat/" + peace.src);
                spite.PositionX = (spite.ContentSize.Width) / 2 + peace.x;
                spite.PositionY = _currentImage.h - peace.y - (spite.ContentSize.Height -1) / 2;
                Log.Info("adyga_pazzle", "Adding spyte {0} content size X:{1} Y:{2} W:{3} H:{4}", prefix + peace.src, spite.PositionX, spite.PositionY, spite.ContentSize.Width, spite.ContentSize.Height);
                AddChild(spite);
                int n;
                if (int.TryParse(peace.name, out n))
                {
                    Log.Info("adyga_pazzle", "Save movable spite with name {0}", peace.name);
                    _peaces.Add(new Peace(spite,  spite.Position));
                }
            }

            // Calculate exploded location
            _peaces[0].DisassembledPos = new CCPoint(100, 50);
            _peaces[4].DisassembledPos = new CCPoint(820, 120);
            _peaces[2].DisassembledPos = new CCPoint(100, 430);
            _peaces[1].DisassembledPos = new CCPoint(860, 450);
            _peaces[3].DisassembledPos = new CCPoint(100, 250);

            // --- End

            // Register for touch events
            var touchListener = new CCEventListenerTouchAllAtOnce();
            touchListener.OnTouchesEnded = OnTouchesEnded;
            touchListener.OnTouchesMoved = OnTouchesMoved;
            touchListener.OnTouchesBegan = OnTouchesBegan;
            AddEventListener(touchListener, this);
        }

        void breakToPeaces()
        {
            CCAudioEngine.SharedEngine.PlayEffect(filename: "break");
            foreach (var p in _peaces)
            {
                DisassemblePease(p);
            }
        }

        void ScheduleAction(Action f, int milliseconds)
        {
            var delayTimer = new System.Timers.Timer(milliseconds);
            delayTimer.AutoReset = false;
            delayTimer.Elapsed += (sender, args) => {
                (sender as Timer).Dispose();
                f();
            };
            delayTimer.Start();
        }

        void ScheduleBreak()
        {
            ScheduleAction(breakToPeaces, 3000);
        }

        void Assemble()
        {
            foreach (var p in _peaces)
            {
                p.Sprite.Position = p.AssembledPos;
            }
        }
        void PlayBackgroundMusic()
        {
            try
            {
                CCAudioEngine.SharedEngine.BackgroundMusicVolume = 0.1f;
                CCAudioEngine.SharedEngine.EffectsVolume = 1.0f;
                CCAudioEngine.SharedEngine.PlayBackgroundMusic(filename: "background_theme", loop: true);
            }
            catch (Exception ex)
            {
                Log.Info("adyga_pazzle", "[ERROR] can't play background music {0}", ex);
            }
        }

        void Challenge()
        {
            const int MAX_CHALLENGE_COUNT = 2;
            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            var index = rnd.Next(1, MAX_CHALLENGE_COUNT + 1);
            CCAudioEngine.SharedEngine.PlayEffect(filename: string.Format("challenge{0}", index));
        }

        public void StartGame()
        {
            Assemble();
            ScheduleBreak();
            Challenge();
        }

        void DisassemblePease(Peace p)
        {
            var easeMove = _factory(new CCMoveTo(1, p.DisassembledPos));
            p.Sprite.RunAction(easeMove);
        }

        void OnImageAssembled()
        {
            const int MAX_APPROVE_COUNT = 7;
            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            var index = rnd.Next(1, MAX_APPROVE_COUNT + 1);
            CCAudioEngine.SharedEngine.PlayEffect(filename: string.Format("approve{0}", index));
            ScheduleAction(()=>{
                CCAudioEngine.SharedEngine.PlayEffect("happykids");
            },1500);
        }

        void StarsFireworks(CCPoint pos)
        {
            try
            {
                var sparks = new CCParticleSystemQuad("sparks.plist");
                sparks.Position = pos;
                AddChild(sparks);
            }
            catch(Exception ex)
            {
                Log.Info("adyga_pazzle", "Can't add sparks to scene {0}", ex);
            }
        }

        void AssemblePeace(Peace p)
        {
            CCAudioEngine.SharedEngine.PlayEffect(filename: "success_partial");
            var easeMove = _factory(new CCMoveTo(0.3f, p.AssembledPos));
            p.Sprite.RunAction(easeMove);
            StarsFireworks(p.AssembledPos);
            var grouped = _peaces.All(s => isPeaceCloseToHome(s));
            if (grouped)
            {
                ScheduleAction(OnImageAssembled, 500);
            }
        }

        void OnTouchesEnded(List<CCTouch> touches, CCEvent touchEvent)
        {
            if (touches.Count > 0)
            {
                if (_spiteToDrag.HasValue)
                {
                    if (!isPeaceCloseToHome(_spiteToDrag.Value.Peace))
                    {
                        DisassemblePease(_spiteToDrag.Value.Peace);
                        CCAudioEngine.SharedEngine.PlayEffect(filename: "fail");
                    }
                    _spiteToDrag = null;
                }
            }
        }

        void OnTouchesBegan(List<CCTouch> touches, CCEvent touchEvent)
        {
            // We only care about the first touch:
            var touch = touches[0];
            foreach (var p in _peaces)
            {
                if (isTouchingPeace(touch, p.Sprite) && !isPeaceAtHome(p))
                {
                    _spiteToDrag = new DraggingSpite(p, touch.Location);
                }
            }

        }

        void OnTouchesMoved(System.Collections.Generic.List<CCTouch> touches, CCEvent touchEvent)
        {
            // We only care about the first touch:
            var locationOnScreen = touches[0].Location;
            if (_spiteToDrag.HasValue)
            {
                var value = _spiteToDrag.Value;
                value.Peace.Sprite.Position = value.StartPosition + locationOnScreen - value.DragStart;
                if (isPeaceCloseToHome(value.Peace))
                {
                    AssemblePeace(value.Peace);
                    _spiteToDrag = null;
                }
            }
        }

        bool isTouchingPeace(CCTouch touch, CCSprite peace)
        {
            // This includes the rectangular white space around our sprite
            return peace.BoundingBox.ContainsPoint(touch.Location);
        }

        bool isPeaceAtHome(Peace peace)
        {
            return peace.Sprite.Position == peace.AssembledPos;
        }

        bool isPeaceCloseToHome(Peace peace)
        {
            const int MIN_DISTANCE = 20;
            return CCPoint.Distance(peace.Sprite.Position, peace.AssembledPos) < MIN_DISTANCE;
        }

        EasingFactory _factory = null;
        public void SetFactory(EasingFactory f)
        {
            _factory = f;
        }
    }
}

