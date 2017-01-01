/****************************************************************************
Copyright (c) 2010-2012 cocos2d-x.org
Copyright (c) 2008-2009 Jason Booth
Copyright (c) 2011-2012 openxlive.com

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CocosSharp;

namespace BouncingGame
{
    public class MenuLayer2 : CCLayer
    {
        MainActivity _activity;
        public MenuLayer2(MainActivity activity) : base()
        {
            _activity = activity;
        }


        protected override void AddedToScene()
        {
            base.AddedToScene();
            var label = new CCLabel("Go To Menu", "Arial", 50, CCLabelFormat.SystemFont);
            var back = new CCMenuItemLabel(label, this.backCallback);
            CCMenu menu = new CCMenu(back) { Tag = 36 }; // 9 items.
            back.PositionY = -250;
            back.PositionX = 0;
            AddChild(menu);
        }

        public void menuCallback(object pSender)
        {
            //UXLOG("selected item: %x index:%d", dynamic_cast<CCMenuItemToggle*>(sender)->selectedItem(), dynamic_cast<CCMenuItemToggle*>(sender)->selectedIndex() ); 
        }

        public void backCallback(object pSender)
        {
            _activity.RunMenu();
        }
    }
}
