# ar-lv-game
My Implementation of a Serious Game in Augmented Reality for Low Vision Rehabilitation in the form of an escape room.

The escape room contains three Gamified Rehabillitation Tasks (GRTs) to be solved in a specific order; the escape room can be solved in two different mode, either using a "press" (buttons) gesture, or a "pinch & slide" (sliders).


## Escape Room: main elements

Description of the final state machine (FSM) running the overall application, the FSM for the escape room itself, and the FSM of each tasks.

### Application

enum GameStates:
- HOME: from here, you can navigate into the TUTORIAL, the CREATION (place the tasks in your environment) and once created the ESCAPEROOM itself.
- TUTORIAL: a brief introduction into the "press" gesture with a button, and the "pinch & slide" gesture with a slider.
- CREATION: let you grab the tasks and place them whenver you deem fit. Click save to enable the start of the escape room.
- ESCAPEROOM: let the fun begin!

### Escape Room

enum EscapeRoomState:
- READY: the tasks have been placed.
- WELCOME: the player entered the escape room
- PLAYING: the player has effectively started the game once she has read the welcome message (and clicked OK)
- PAUSE: whenever the player exits the game
- SOLVED: once all three tasks have been solved

### Gamified Rehabilitation Tasks (GRTs)

The escape room is composed of three GRTs. There can be a version of the escape room's GRTs with 'press' or 'pinch & slide' gestures; which gesture is used depends on the type of 'Controller' inside the GRT.

Under Assets, in the [GRTs' folder](./Assets/Prefabs/GRTs/), each GRT has its own folder which includes:
- a base Prefab without any controller
- a controller for the 'press' gesture
- a controller for the 'pinch & slide' gesture

Once the base Prefab is placed in the Scene's Hierarchy, choose one type of controller and place it inside the child 'Controller' of the desired base Prefab.

Regarding the [GRTs' scripts](./Assets/Scripts/GRTs/), the class ```GRTGeneric<T> : MonoBehaviour```  is the parent of all tasks in the escape room. It is responsible to find the 'Controller', 'Core', 'Support' and 'ButtonStart' in the parent's GameObject. This class is also responsible to create a ```FiniteStateMachine<GRTState>```.


## Creation of a new task
A task, called Gamified Rehabilitation Task (GRT), can be created in Unity using the following steps:

1. Create an empty GameObject which will represent the parent of the GRT
2. Inside it, create the following children respecting the names (i.e. the script below will need them)
   1. "Controller": the buttons or sliders required to solve the task
   2. "Core": the GameObjects representing the actual task
   3. "Support": any background or support that helps to hold or place the task
   4. "ButtonStart": a button to start the task
3. Create a new script, which inherit from GRT_Press or GRT_PinchSlides depending on which interaction mode you choose
   1. modify the Start():
   ```C#
   override void Start():
   {
    base.Start();   // Will call the Start() methods of the parents
    ...             // any of your code here
   }
   ```
4. Attach the new script to the parent GameObject
5. 