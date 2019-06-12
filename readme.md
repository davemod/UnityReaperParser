# UnityReaperParser

UnityReaperParser is a tool for Unity written in C# to load and parse Reaper Project Files (.rpp/.RPP). This can be used to build your own tool to use Reaper as a kind of middleware to implement audio in video games. 

## Getting Started

### Installing

Download the repository and import the src folder and the SampleScene into your project. 

##### Set the correct path

Open the SampleScene in Unity. In the hierarchy, click the "Main Camera" object. In the inspector find the "Reaper Parser Component" and paste the absolute path to the reaper file in the downloaded "Path To Reaper File".

## How does it work


### The Reaper File .rpp

To use the UnityReaperParser it is necessary to understand how reaper stores it's state. UnityReaperParser reads the .rpp like a .txt. You can open this file in a text editor of your choice. In this file we can find all information about our reaper session, the tracks, the regions, the automation etc. and use it in our video games.

```
<REAPER_PROJECT 0.1 "5.978/OSX64" 1558100284
  RIPPLE 0
```

These are the first two lines of our project file. The first says: here starts a new node of type ```REAPER_PROJECT``` with the values ```0.1 "5.978/OSX64" 1558100284```. The second value for example tells us about the Reaper version, the third value is the time we saved our file (in milliseconds since 1970).

The first child node inside is ```RIPPLE``` with it's value ```0``` which tells us about the ripple editing mode we selected in our reaper session. Try it yourself: open the .rpp file in Reaper and enable <i>Options -> Ripple edit per-track</i> and save the session. The line should now be ```RIPPLE 1```.

Inside the ```REAPER_PROJECT``` we find other children like tracks:

```
<TRACK {0A7FB69C-408E-214A-B7D2-977DCD7A7633}
    NAME Ambient
    ...
```

In line 87 we find our first ```TRACK``` with it's ID ```{0A7FB69C-408E-214A-B7D2-977DCD7A7633}``` as it's first value. ```NAME```is a child node of this track, just like the ```TRACK``` itself is a child of the ```REAPER_PROJECT```.

### Use UnityReaperParser

Find the script ReaperParserComponent.cs and open it. You will find the method Example().

```
            // open the file and parse it
            parser = new ReaperParser(pathToReaperFile);

            if (!parser.isValid)
            {
                Debug.LogError("The ReaperParser was not initialized.");
                return;
            }
            
            // get the parsed main ReaperNode object
            rpp = parser.rpp;
```

The above is how to instantiate the ReaperParser. If UnityReaperParser found the file and parsed it correctly isValid will be true.


```
            //e.g. get the position of the cursor. There is only one value. Value returns always the first of all values.
            string cursorPosition = rpp.GetNode("CURSOR").value;
            Debug.Log("The cursor position: " + cursorPosition);

            // e.g. get the main tempo information of the session.
            List<string> tempo = rpp.GetNode("TEMPO").values;
            Debug.Log("Tempo information: " + string.Join(", ", tempo.ToArray()));

```
The second part demonstrate how to get the position of the cursor or information about the global bpm settings. This is why you need to understand how the .rpp file works. For now you get all the information you need by calling ```GetNode("NODE_TYPE")``` You need to be sure that there is a node with the passed type otherwise GetNode returns ```null```. 

As a last example on how to dig through the rpp file:

```
 // e.g. get the name of the first item on the last track
            string itemName = rpp.GetLastNode("TRACK").GetNode("ITEM").GetNode("NAME").value;
            Debug.Log("name of the first item on the last track: " + itemName);
```

### Get an idea how to use this

In our SampleScene in Unity there are to objects with SoundSources – "Ambient" and "Gunshot". In ReaperParserComponent.cs you can find the IEnumerator LoadAudio(). In this example we use UnityReaperParser to iterate through all the tracks in the reaper file. We load the first item on the track from disk and assign it to the corresponding Unity Objects.

This is an example how to create your own middleware using UnityReaperParser. Here we create new GameObjects for our tracks in Reaper and assign the first item of each track as an AudioClip to the AudioSource. This one is aasy, but there are no limits to your creativity.

```
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
```

To really use this tool as a middle ware there is far more work to do but it's a start.

## Authors

* **David Hill** - *Initial work* - [davemod](https://github.com/davemod)

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details