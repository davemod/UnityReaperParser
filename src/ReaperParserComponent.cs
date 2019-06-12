/** @file ReaperParserComponent.css
 *  @brief Demonstration of how to use an instance of ReaperParser
 *  @author David Hil
 *
 *  Copyright (c) 2019 David Hill
 *  
 *  Permission is hereby granted, free of charge, to any person obtaining a copy
 *  of this software and associated documentation files (the "Software"), to deal
 *  in the Software without restriction, including without limitation the rights
 *  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 *  copies of the Software, and to permit persons to whom the Software is
 *  furnished to do so, subject to the following conditions:
 *  
 *  The above copyright notice and this permission notice shall be included in all
 *  copies or substantial portions of the Software.
 *  
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 *  SOFTWARE.
 */
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityReaperParser
{
    public class ReaperParserComponent : MonoBehaviour
    {
        public string pathToReaperFile;

        public ReaperParser parser;

        public ReaperNode rpp;

        public AudioSource ambientSource;
        public AudioSource gunshotSource;

        // Use this for initialization
        void Start()
        {
            Example();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                gunshotSource = GameObject.Find("Gunshot").GetComponent<AudioSource>();
                if (gunshotSource)
                    gunshotSource.Play();
            }
        }

        /// <summary>
        /// Parse the rpp file.
        /// </summary>
        private void Example(){
            
            // open the file and parse it
            parser = new ReaperParser(pathToReaperFile);

            if (!parser.isValid)
            {
                Debug.LogError("The ReaperParser was not initialized.");   
                return;
			}
            
            // get the parsed main ReaperNode object
            rpp = parser.rpp;

            // e.g. get the position of the cursor. There is only one value. Value returns always the first of all values.
            string cursorPosition = rpp.GetNode("CURSOR").value;
            Debug.Log("The cursor position: " + cursorPosition);

            // e.g. get the main tempo information of the session.
            List<string> tempo = rpp.GetNode("TEMPO").values;
            Debug.Log("Tempo information: " + string.Join(", ", tempo.ToArray()));

            // e.g. get the name of the first item on the last track
            string itemName = rpp.GetLastNode("TRACK").GetNode("ITEM").GetNode("NAME").value;
            Debug.Log("name of the first item on the last track: " + itemName);

            // load the audio in a coroutine to be sure the audioclip is loaded before assigned to a audiosource
            StartCoroutine(LoadAudio());
          
        }

        IEnumerator LoadAudio()
        {

            var tracks = rpp.GetNodes("TRACK");

            /* 
             * Iterate through all the tracks in the reaper file. Then
             * find the corresponding Gameobject in the Unity Scene. Load the 
             * first item on the track and assign it to the corresponding 
             * GameObject in the scene. 
             * 
             * This is an example how to create your own middleware using 
             * UnityReaperParser. Here we use the names of the GameObjects 
             * and the names of the tracks in reaper to assign audio to 
             * AudioSources. This one is aasy, but there are no limits to 
             * your creativity.
            */

            var reaperObject = GameObject.Find("Reaper");

            foreach (var t in tracks)
            {

                var name_ = t.GetNode("NAME").value;
                var item_ = t.GetNode("ITEM");

                var g = new GameObject(name_);
                g.transform.parent = reaperObject.transform;
                g.AddComponent<AudioSource>();

                //var g = GameObject.Find(name_);

                if (item_ == null || g == null)
                    continue;

                var src_ = g.GetComponent<AudioSource>();

                if (src_ == null)
                    continue;

                var container = new ReaperParser.Container<AudioClip>();
                yield return ReaperParser.LoadAudioFromDisk(item_, container);

                src_.clip = container.t;
                src_.loop = item_.GetNode("LOOP").value == "1" ? true : false;
                src_.Play();
            }
        }
	}
}