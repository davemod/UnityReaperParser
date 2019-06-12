/** @file ReaperParser.css
 *  @brief Implementation of the class ReaperParser to read and parse an .rpp file.
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


using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityReaperParser
{
    public partial class ReaperParser
    {
        public ReaperNode rpp;
            
        private string  content;
        public string   path { get; private set; }
        public string   directory { get; private set; }
		public bool     isValid { get; private set; }


		private string[]    contentLines;
        private int         currentLine = 0;
        private ReaperNode  currentNode;


        /// <summary>
        /// Initializes a new instance of the <see cref="T:UnityReaperParser.ReaperParser"/> class.
        /// </summary>
        /// <param name="path">The Absolute Path to the Reaper File .rpp.</param>
        public ReaperParser(string path)
        {
            // Read File and store it to member content
            try
            {
                StreamReader sf = new StreamReader(path);
                this.content = sf.ReadToEnd();
                this.contentLines = content.Split('\n');
                rpp = Parse(null, this);
                this.path = path;
                this.directory = Path.GetDirectoryName(path);

                isValid = true;
            }
            catch (Exception e)
            {
                isValid = false;
                Debug.LogError(e.ToString());
            }
        }

        /// <summary>
        /// Parse the reaper file.
        /// </summary>
        /// <returns>The top node.</returns>
        /// <param name="parent">Parent.</param>
        /// <param name="parser">The parsing ReaperParser.</param>
        private ReaperNode Parse(ReaperNode parent, ReaperParser parser){

            string cl = contentLines[currentLine];

            if (cl.Contains("<")){
                var newNode = new ReaperNode(cl, parent,parser);   // Create a new Node
                if (currentNode!=null)                      // only happens for the first line <REAPER_PROJECT
                    currentNode.addChild(newNode);
                currentNode = newNode;                      // Set the current node
            } else if (cl.Contains(">")){
                currentNode = currentNode.parent != null ? currentNode.parent : currentNode;
            } else if (currentNode != null){
                currentNode.addChild(new ReaperNode(cl, currentNode,parser));
            }

			currentLine++;

            if (currentLine < contentLines.Length)
                Parse(currentNode,this);

            return currentNode;
        }
    }




    public partial class ReaperParser
    {
        /// <summary>
        /// Container to pass certain data types to IEnumerators as reference.
        /// </summary>
        public class Container<T>
        {
            public T t;
        }

        /// <summary>
        /// Loads the audio from disk using the WWW class. Adds the loaded file
        /// as an AudioClip to the Container<AudioClip>.
        /// </summary>
        /// <param name="item">The ReaperNode instance with type "ITEM".</param>
        /// <param name="clipContainer">An instance of ClipContainer<AudioClip>.</param>
        public static IEnumerator LoadAudioFromDisk(ReaperNode item, Container<AudioClip> clipContainer)
        {
            if (item.type == "ITEM")
            {
                var source_wave = item.GetNode("SOURCE");
				var source_path = source_wave.GetNode("FILE").value;

				if (source_path[0] != '/') // is relative path
					source_path = item.parser.directory + "/" + source_path;

                string url = "file:///" + source_path;
                WWW www = new WWW(url);

                clipContainer.t = www.GetAudioClip();

                while (!clipContainer.t.isReadyToPlay)
					yield return null;
            }
        }
    }
}