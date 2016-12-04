using System;
using System.Collections.Generic;
using CocosSharp;
using Microsoft.Xna.Framework;
using Android.Util;
using System.IO;

namespace BouncingGame
{
    struct DraggingSpite
    {
        public DraggingSpite(CCSprite p, CCPoint s)
        {
            Peace = p;
            StartPosition = p.Position;
            DragStart = s;
        }
        public CCSprite Peace;
        public CCPoint StartPosition;
        public CCPoint DragStart;
    }

    public class GameLayer : CCLayerColor
    {
        MainActivity _parent;
        bool _isGrouped = true;
        image _currentImage;
        List<KeyValuePair<CCSprite, CCPoint>> _peaces = new List<KeyValuePair<CCSprite, CCPoint>>();
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
                    _peaces.Add(new KeyValuePair<CCSprite, CCPoint>(spite,  spite.Position));
                }
            }
            // --- End

            // Register for touch events
            var touchListener = new CCEventListenerTouchAllAtOnce();
            touchListener.OnTouchesEnded = OnTouchesEnded;
            touchListener.OnTouchesMoved = OnTouchesMoved;
            touchListener.OnTouchesBegan = OnTouchesBegan;
            AddEventListener(touchListener, this);
        }

        void Explode()
        {
            {
                var action = new CCMoveTo(1, new CCPoint(100, 50));
                _peaces[0].Key.RunAction(action);
            }
            {
                var action = new CCMoveTo(1, new CCPoint(820, 120));
                _peaces[4].Key.RunAction(action);
            }
            {
                var action = new CCMoveTo(1, new CCPoint(100, 430));
                _peaces[2].Key.RunAction(action);
            }
            {
                var action = new CCMoveTo(1, new CCPoint(860, 450));
                _peaces[1].Key.RunAction(action);
            }

            {
                var action = new CCMoveTo(1, new CCPoint(100, 250));
                _peaces[3].Key.RunAction(action);
            }
        }

        void Collapse()
        {
            foreach (var kv in _peaces)
            {
                var action = new CCMoveTo(1, kv.Value);
                kv.Key.RunAction(action);
            }
        }

        void OnTouchesEnded(List<CCTouch> touches, CCEvent touchEvent)
        {
            if (touches.Count > 0)
            {
                if (_isGrouped)
                {
                    Explode();
                    _isGrouped = false;
                }
                else
                {
                    _spiteToDrag = null;
                }
            }
        }

        void OnTouchesBegan(List<CCTouch> touches, CCEvent touchEvent)
        {
            if (_isGrouped)
                return;
            // We only care about the first touch:
            var touch = touches[0];
            foreach (var p in _peaces)
            {
                if (isTouchingPeace(touch, p.Key))
                {
                    _spiteToDrag = new DraggingSpite(p.Key, touch.Location);
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
                value.Peace.Position = value.StartPosition + locationOnScreen - value.DragStart;
            }
        }

        bool isTouchingPeace(CCTouch touch, CCSprite peace)
        {
            // This includes the rectangular white space around our sprite
            return peace.BoundingBox.ContainsPoint(touch.Location);
        }
    }
}

