# XR4MCR
## MULTI-USER COLLABORATIVE ROBOTIC MAINTENANCE TRAINING PLATFORM IN EXTENDED REALITY

XR4MCR is an innovative extended reality (XR) platform developed by Lider Teknoloji Geliştirme Ltd. Şti. under the MASTER-XR open call. It is designed to provide collaborative training in a multi-user environment for industrial robotic maintenance training. The platform uses Virtual Reality (VR) and Mixed Reality (MR) technologies to create immersive training experiences. The project aims to provide the ability to design and implement industrial robot maintenance training in a mixed reality environment without requiring coding.

![XR4MCR Application Image 1](https://github.com/user-attachments/assets/3778fb0d-28f1-40c6-b670-c2169c814443)
![XR4MCR Application Image 2](https://github.com/user-attachments/assets/af652ebe-8a8e-4eca-aaee-aaa40bc7f836)

## Software Description

XR4MCR provides a training environment where multiple users can simultaneously interact with industrial robotic systems in a virtual space. The platform supports:

- Real-time collaboration between multiple users
- Detailed industrial robot models with interactive components
- Step-by-step maintenance procedure guidance using a node-based visual programming system
- Performance monitoring and evaluation
- Cross-platform compatibility (VR headsets and MR devices)
- Creating training content without coding

## Technical Features

**Built with Unity Game Engine**: XR4MCR is built on the Unity game engine, which offers rich XR support and a vast ecosystem. It uses a Render Pipeline optimized for mixed reality applications.

**VR/MR Integration via OpenXR**: Uses OpenXR for standardized XR device interaction. It provides a high-quality mixed reality experience by blending virtual content with the real environment, especially with the passthrough technology of the HTC Vive Focus Vision headset.

**Multi-User Network Capabilities**: Provided through VIROO platform integration. This integration enables multi-user functionality, session management, and content distribution infrastructure. It supports real-time collaboration and sharing of training scenarios.

**Realistic Physics Simulation**: Enhances immersion and interaction in the training environment.

**Interactive 3D Interfaces**: Provided through a multi-canvas system (Editor, Object, and Information Canvases) optimized for mixed reality environments.

**Model-View-Presenter (MVP) Architectural Approach**: Ensures code base modularity, testability, and maintainability. The architecture includes Model layers for data/business logic, View for UI components, and Presenter for mediation.

**Extensible Modular Architecture with Zenject Dependency Injection**: Provides loose coupling between components, offering cleaner code and easier testability.

**Advanced Axis-Based Transformation Systems**: Used for precise manipulation of objects in the environment.

**Integrated XML Serialization Structure**: Enables saving and loading scenarios by combining nodes, connections, and scene objects in a single structure.

**3D Model Management System**: Integrated with Nextcloud, enabling remote storage, management, and dynamic downloading of 3D models used in training scenarios. This keeps the application's initial size small and allows models to be updatable.

## Installation

1. Clone the repository:
```bash
git clone https://github.com/LiderTeknolojiGelistirme/XR4MCR.git
```

2. Open the project in Unity (2022.3.40f or later recommended).

3. Install required packages through Unity Package Manager:
   - XR Interaction Toolkit
   - OpenXR Plugin
   - VIVE OpenXR Plugin

## Git LFS Information

This repository uses Git Large File Storage (Git LFS) to efficiently manage large files. The following file types are tracked with Git LFS:

- 3D Models (*.fbx)
- Textures (*.png, *.jpg, *.tga, *.tif, *.tiff, *.exr, *.hdr)
- Unity Assets (*.unity, *.asset, *.prefab, *.mat)
- Audio files (*.wav, *.mp3, *.ogg)
- Video files (*.mp4, *.mov)
- Compiled libraries (*.dll)
- Archives (*.zip)

When working with this repository, ensure Git LFS is installed:
```bash
git lfs install
```

**Note**: Some large files exceeding GitHub's size limits have been excluded from the repository.

## Usage

The XR4MCR platform offers a user-friendly interface for creating and executing complex training scenarios without requiring coding.

1. Launch the project in Unity.
2. Configure your VR/MR devices in the Unity XR settings.
3. Enter play mode to start the simulation.
4. Follow the in-app tutorial for a guided experience.

### Scenario Creation Workflow

The process of creating a training scenario proceeds as follows:

1. The user creates a scenario flow starting with a "Start Node" on the editor canvas.
2. 3D models are selected from the "Object Canvas" and placed in the scenario area.
3. Nodes representing different actions and logic (e.g., TouchNode, GrabNode, LookNode, Logical AND/OR Node, ActionNode for audio, position, rotation, scale, material, description) can be added and configured as tasks.
4. Nodes can be connected to define the flow of the scenario. For instance, an Audio Action Node can be connected to a Touch Node to play a sound upon task completion.
5. The scenario is ended with a Finish Node.
6. The entire scenario is saved in XML format and can be shared.

## Core User Interface (UI) Systems

- **Editor Canvas**: The main interface for visually programming training scenarios using nodes.
- **Object Loading Canvas**: Allows users to find, preview, select, and import 3D models into the editing area from a centralized Nextcloud repository.
- **Information Canvas**: Provides real-time information such as scenario status, active node information, and system messages.
- **Scenario Playback Area**: The workspace where the training scenario is executed, allowing user interaction with 3D objects and monitoring the scenario flow.

## Node Types and Features

XR4MCR offers various node types to support different training scenario steps:

| Node Type | Function | Special Features |
|-----------|----------|------------------|
| StartNode | Scenario starting point | Includes output ports only |
| FinishNode | Scenario endpoint | Includes input ports only |
| TouchNode | Touching an object | Object to be touched (TargetObjectID) |
| GrabNode | Object gripping and moving | Target object (TargetObjectID) |
| LookNode | Looking at an object | Object being looked at (TargetObjectID) |
| LogicNode | Logical operations | Operator Type (AND, OR) |
| ActionNode | Basic action node | Type, TargetObjectID, ParameterName, ParameterValue |
| AudioActionNode | Advanced sound-related actions | DropdownItems (list of audio options) |
| ChangeMaterialAction | Material changing | TargetObjectID, ParameterValue (material) |
| ChangePositionAction | Position changing | TargetObjectID, ParameterValue (target position) |
| ChangeRotationAction | Rotation changing | TargetObjectID, ParameterValue (target rotation) |
| ChangeScaleAction | Scale changing | TargetObjectID, ParameterValue (target scale) |
| DescriptionActionNode | Displaying text description | ParameterValue (text to display) |

### Port and Connection System

Data flow and logical relationships between nodes are provided through the port and connection system. This system manages the flow control of scenarios.

| Port Polarity Types | Description | Connection Rules |
|-------------------|-------------|------------------|
| Input | Input port | Can only receive connections from Output ports |
| Output | Output port | Can only connect to Input ports |
| Bidirectional | Bi-directional port | Can connect in both directions |

### Event System

XR4MCR's node system uses an event-based communication mechanism. This mechanism allows nodes to communicate with each other without direct connection.

| Event Type | Trigger Time | Purpose |
|------------|--------------|---------|
| OnStarted | When the node starts working | Operations such as starting animations, playing sounds |
| OnCompleted | When the node is completed | Transition to the next node |
| OnSkip | When a node is skipped | What to do in case of a skip |



## Requirements

- Unity 2022.3.40f (tested) or later
- Compatible VR headset or MR device (e.g., HTC Vive Focus Vision)
- OpenXR compatible runtime
- Windows 10 or later
- Working environment with approximately 2m x 3m clear space for mixed reality scenarios
- **Development Tools**: Visual Studio 2022 (IDE), Unity Editor (2022.3.40f), Unity Hub (3.11.1)
- **Version Control and Documentation Tools**: Subversion (SVN) with TortoiseSVN client, DOORS (requirements management), Teams (project documentation), Redmine (task tracking)
- **Model and Content Creation Tools**: Blender (3D model editing), FreeCAD (CAD conversion), Adobe Photoshop (visual editing)


## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to your branch
5. Create a new Pull Request

## License

This project is licensed under the Apache-2.0 License - see the LICENSE file for details.

## Acknowledgements

This work is co-funded by the European Union (EU) under grant number 101093079 (project MASTER, https://www.master-xr.eu).


# MORE DETAILS

# GENERAL SYSTEM OVERVIEW

The XR4MCR system is a comprehensive mixed reality training platform designed for industrial robot maintenance training. It integrates multiple technologies and components to create an interactive, collaborative learning environment without requiring programming knowledge.

![image](https://github.com/user-attachments/assets/86559b1e-224d-4b90-92de-52690f1267b3)

Figure 0: XR4MCR Application Screenshot

## System Architecture Overview

XR4MCR is built on a modular architecture that combines:

1. **Node-Based Visual Programming System**: The core of the platform, enabling trainers to create interactive training scenarios by visually connecting nodes that represent different actions and logic.
2. **Mixed Reality Interface**: Utilizing the HTC Vive Focus Vision headset's passthrough capability to blend virtual content with the real environment, allowing for immersive training experiences.
3. **VIROO Platform Integration**: Providing multi-user functionality, session management, and content delivery infrastructure.
4. **3D Model Management System**: Integrated with Nextcloud for remote storage, management, and download of 3D models used in training scenarios.

## Key System Components

### Software Components

- **Unity Engine (2022.3.40f LTS)**: The development platform.
- **MVP Architecture**: Implemented across three distinct layers:
  - Model Layer: Data structures and business logic in pure C# classes
  - View Layer: Unity UI components and prefabs for visualization
  - Presenter Layer: Mediating between model and view, handling interactions
- **Zenject Dependency Injection Framework**: Managing dependencies and providing a loosely coupled, modular architecture.
- **Node System**: A comprehensive set of node types (StartNode, ActionNode, LogicNode, etc.) that enable the creation of complex training scenarios through visual connections.
- **Canvas Systems**: Three main interactive canvases:
  - Editor Canvas: For creating and editing node-based scenarios
  - Object Canvas: For selecting and managing 3D models
  - Information Canvas: For displaying scenario information and guidance
- **Serialization System**: XML-based storage solution for saving and loading complete training scenarios, including node configurations, connections, and 3D object placements.

### ****Hardware Components:****

- **HTC Vive Focus Vision**: Mixed reality headset providing passthrough capability for blending virtual content with the real world.
- **High-Performance Workstations**: Required for development and testing of mixed reality applications.

## System Workflow

The XR4MCR platform operates through the following high-level workflow:

1. **Training Design Phase:**

- Trainers create scenarios using the node-based editor
- 3D models are imported from the Nextcloud repository and placed in the scenario area
- Node connections define the logical flow of the training scenario
- Complete scenarios are saved in XML format

1. **Training Execution Phase:**

- Users access the training through the VIROO platform
- The scenario loads with all node configurations and 3D objects
- Users follow the training steps through mixed reality interaction
- Progress is tracked and managed by the system

1. **Multi-User Collaboration:**

- Multiple users can join the same training session
- Instructors can guide and observe trainees in real-time
- All users see the same virtual content synchronized across devices

## Technical Integration Points

The system integrates several key technologies:

- **VIROO SDK**: For multi-user capability and industrial training platform integration
- **OpenXR and XR Interaction Toolkit**: For standardized XR device interaction
- **Nextcloud WebDAV Protocol**: For 3D model repository access
- **XML Serialization**: For scenario data storage and retrieval
- **Unity Addressable Assets System**: For dynamic loading of 3D models at runtime

The XR4MCR system represents a comprehensive solution for creating and delivering industrial robot maintenance training in a mixed reality environment. Its modular architecture and no-code approach make it accessible to trainers without programming skills while providing the flexibility and power needed for complex training scenarios.


# SOFTWARE ARCHITECTURE

![image](https://github.com/user-attachments/assets/07b17125-561e-4d48-849b-0f204a47a3ab)

Figure 1: Scenario Scene

## Architectural Overview

### Architectural Approach and Design Principles

The XR4MCR platform has a robust software architecture for creating and executing complex training scenarios. The system is designed to provide modularity, flexibility, and ease of maintenance based on the Model-View-Presenter (MVP) architectural pattern.

#### MVP Architecture Implementation

In XR4MCR, the MVP architecture is implemented in a way that is adapted to Unity's characteristics compared to the traditional structure. This architecture is structured as shown in the following figure:

![image](https://github.com/user-attachments/assets/3e643782-7c9b-44d5-9ea9-53d74d6dde18)

Figure 2: XR4MCR MVP Architecture Basic Structure

##### Model Layer

The model layer contains the application's data structures and business logic. Model classes do not contain any Unity dependencies and are implemented as pure C# classes. This ensures that the model can be tested independently and reused in different environments.

##### View Layer

The View layer consists of Unity's UI components and prefabs. This is where XR4MCR differs from the traditional MVP; instead of a custom View layer code, it uses Unity's built-in UI system and scene components. This layer includes:

- Canvas and UI elements (panels, buttons, text fields)
- Prefabs for visual representations of nodes and connections
- XR interaction components
- 3D objects and scenes

##### Presenter Layer

The Presenter layer manages the communication between the Model and the View. This layer:

- Takes model data and reflects it in the user interface
- Processes user interactions and updates the model
- Manages system state

#### Basic Design Principles and Practices

The basic principles applied in the architectural design of the XR4MCR and their application in the project are detailed in the table below:

Table 1: XR4MCR Design Principles and Practices

| Design Principle | Application in XR4MCR | Example |
| --- | --- | --- |
| Modularity | The system is composed of independent modules | Node system, UI system and XR interaction system can work independently of each other |
| Single Responsibility Principle | Each class and component performs a single task | The GraphManager only deals with the editorial canvas, and the ScenarioManager only deals with the scenario flow |
| Factory Pattern | The creation of objects is carried out in centralized factories | NodePresenterFactory and ConnectionPresenterFactory classes |
| Dependency Injection | Dependencies are centrally managed with Zenject | The GraphSceneInstaller class configures all system dependencies. |
| Observer Pattern | Communication between components is provided with the event system | OnActivated, OnCompleted events of nodes |
| Command Pattern | User actions are encapsulated in separate command classes | Node action commands (ChangePosition, ChangeRotation, etc.) |

#### Dependency Injection and Zenject

The XR4MCR project uses the Zenject library to manage dependencies between components. This approach reduces tight coupling between components, resulting in a more modular, testable, and easy-to-maintain architecture.

Zenject offers the following key benefits in the project:

1. Loose coupling between components
2. Improved testability
3. Control over system startup order
4. Object lifecycle management

The dependency injection structure of the project is configured through the GraphSceneInstaller class. Detailed application, working principles, and advantages of Zenject in the XR4MCR project are extensively covered in Section 6.2.4.

#### XR4MCR Architectural Flowchart

The following diagram illustrates how user interactions are handled in the system and the underlying data flow:

![image](https://github.com/user-attachments/assets/aaa6d924-005b-4884-b659-f137a7091d54)

Figure 3: Data Flow in XR4MCR System

#### Project-Specific Architectural Decisions

In the XR4MCR project, some specific architectural decisions were made to support a multi-user scenario that will run in a mixed reality environment:

1. **Node-Based Design:** A node-based visual programming approach has been adopted to enable users to create training scenarios without writing code. This allows instructors to easily create interactive scenarios.
2. **Multi-Canvas System:** The user experience in the mixed reality environment has been optimized by using three separate canvas systems: the editor canvas, the object canvas, and the information canvas.
3. **VIROO Integration:** Integration with the VIROO platform is provided to support multi-user interaction. This integration is implemented through a specialized network layer.
4. **XR Interaction Layer:** A dedicated interaction layer has been developed to standardize interaction with XR devices such as HTC Vive. This layer ensures compatibility with different XR devices.

The XR4MCR architecture is designed to support both single-user and multi-user mixed reality scenarios. This architectural approach enables industrial robot maintenance training to be conducted in a realistic and interactive manner, while providing trainers with the ability to easily create and manage scenarios.

### Technology Choices

The XR4MCR project uses various technologies to support multi-user robot maintenance training in a mixed reality environment. This section describes in detail the key technologies, libraries, and tools used in the project.

#### Core Development Platform: Unity

XR4MCR is built on the Unity game engine. The main reasons for choosing Unity are:

- **Rich XR Support:** Unity offers comprehensive support for a variety of XR devices
- **Performance Optimization:** Render pipeline optimized for mixed reality applications
- **Broad Ecosystem:** Accelerating the development process with ready-made assets, packages, and plugins
- **VIROO Integration:** VIROO's ability to integrate with Unity

##### Unity Version and Configuration

Table 2: Unity Configuration

| **Component** | **Version/Detail** | **Purpose** |
| --- | --- | --- |
| Unity Engine | 2022.3.40f LTS | Long-term support and stability, tested VIROO version |
| Render Pipeline | Built-in | XR-optimized performance |
| .NET API Compatibility | .NET Standard 2.1 | Broader C# library support |
| Target Platforms | Windows | PC VR and mobile VR support |

##### Basic Unity Packages

Table 3: Basic Unity Packages Used

| **Package Name** | **Version** |
| --- | --- |
| XR Interaction Toolkit: | 3.0.5 |
| XR Plugin Management: | 4.5.0 |
| OpenXR Plugin: | 1.12.1 |
| Input System: | 1.11.0 |
| TextMeshPro: | 3.0.9 |
| DOTween | 1.2.632 |
| VIVE OpenXR Plugin | 2.5.1 |
| VIVE Wave XR Plugin | 6.2.0-r.9 |
| Viroo Studio | 2.6.934 |
| Zenject | 9.2.0 |

#### VIROO Platform

VIROO Studio 2.6.934 provides the multi-user interaction and content management infrastructure of the XR4MCR project. Reasons for using VIROO in the project:

- **Rapid Prototyping:** VIROO enables rapid prototyping of XR applications
- **Multi-User Support:** Supports the interaction of multiple users in the same XR environment
- **Interaction System:** Provides custom scripts for interaction

Integration with the VIROO platform is implemented as shown in the following architectural diagram:

![image](https://github.com/user-attachments/assets/e907ac82-37de-4e16-9166-01344adee86f)

Figure 4: VIROO Integration Architecture

#### XR Hardware and Passthrough Technology

XR4MCR uses advanced XR hardware to optimize the mixed reality experience. Within the scope of the project, HTC Vive Focus Vision headsets have been preferred. The primary reason for choosing this device is that it provides seamless integration with the VIROO platform and offers a true mixed reality experience by providing high-quality passthrough technology.

##### HTC Vive Focus Vision

HTC Vive Focus Vision is a mixed reality device optimized for industrial training applications. The main advantages of using this device in the XR4MCR project are:

- **VIROO Compatibility:** Supports multi-user mixed reality experiences by providing full integration with the VIROO platform
- **High-Quality Passthrough:** Transmits real-world images with low latency, enabling seamless fusion of virtual and real content
- **Standalone Operation:** Can deliver a mixed reality experience without requiring an external computer
- **Industrial Durability:** Designed for long-term use in training environments
- **Comfortable Design:** Ergonomic structure that provides user comfort during extended training sessions

HTC Vive Focus Vision captures and processes real-world images through its passthrough cameras, seamlessly merging them with the virtual content of the XR4MCR application. This technology enables users to interact with virtual objects in real-world environments, creating an ideal platform for robotic maintenance training.

##### Passthrough Technology

Passthrough technology is the transmission of real-world images to the user by capturing and processing them through the cameras in the XR glasses. Here's how this technology is implemented in XR4MCR:
```
// Sample code from MR_Passthrough.cs

public class MR_Passthrough : MonoBehaviour

{

 private Camera \_xrCamera;

 private Material \_passthrough;



 void Start()

 {

     // Get the XR camera

     \_xrCamera = GetComponent&lt;Camera&gt;();



     // Set the passthrough material

     \_passthrough = new Material(Shader.Find("Hidden/Universal Render Pipeline/XR/Passthrough"));



     // Configure camera settings

     ConfigurePassthroughCamera();

 }



 void ConfigurePassthroughCamera()

 {

     // Set camera background material as passthrough

     \_xrCamera.clearFlags = CameraClearFlags.SolidColor;

     \_xrCamera.backgroundColor = Color.clear;



     // Set renderings for the background camera

     if (\_xrCamera.GetComponent&lt;UniversalAdditionalCameraData&gt;() != null)

     {

         var cameraData = \_xrCamera.GetComponent&lt;UniversalAdditionalCameraData&gt;();

         cameraData.renderPostProcessing = true;

         cameraData.renderShadows = false;

         cameraData.backgroundColorHDR = Color.clear;

     }

 }

}
```
Figure 5: Passthrough System Sample Code

#### Zenject Dependency Injection

XR4MCR uses the Zenject library to manage dependencies between components. Zenject provides the following advantages:

- **Loosely Coupled Components:** Communication between components through abstractions rather than direct references
- **Testability:** Easier test writing thanks to the ability to mock dependencies
- **Lifecycle Management:** Centralized management of object lifecycles
- **Factory Support:** Simplifying complex object creation processes

#### Nextcloud Integration

Nextcloud is used for storing and managing 3D models and training content. Nextcloud integration provides the following advantages:

- **Centralized Content Management:** All 3D models and training content are stored in a central repository
- **Version Control:** Management of different versions of content
- **Access Control:** Role-based control of access to content
- **Real-Time Synchronization:** Synchronization of content between different users

#### Data Serialization Technologies

In XR4MCR, XML serialization is used for data storage and loading operations. This choice was made for the following reasons:

- **Readability:** The XML format is human-readable and editable
- **Structured Data:** Ability to represent hierarchical data structures
- **Broad Support:** Extensive support in the .NET ecosystem
- **Flexibility:** Allows for the addition of new node types or features in the future
```
// Scenario serialization and deserialization example

public void SaveGraph(string filePath)

{

 XmlSerializer serializer = new XmlSerializer(typeof(SaveFile));

 SaveFile saveData = new SaveFile();



 // Get node and connection data

 saveData.Nodes = CollectNodeData();

 saveData.Connections = CollectConnectionData();



 // Save in XML format

 using (FileStream stream = new FileStream(filePath, FileMode.Create))

 {

     serializer.Serialize(stream, saveData);

 }

}

public void LoadGraph(string filePath)

{

 XmlSerializer serializer = new XmlSerializer(typeof(SaveFile));



 // Read XML file

 using (FileStream stream = new FileStream(filePath, FileMode.Open))

 {

     SaveFile loadedData = (SaveFile)serializer.Deserialize(stream);



     // Create nodes and connections

     RecreateNodesFromData(loadedData.Nodes);

     RecreateConnectionsFromData(loadedData.Connections);

 }

}
```
Figure 6: XML Serialization Example

#### Evaluation of Technology Choices

The technologies selected for the XR4MCR project are optimized to support multi-user mixed reality training scenarios. The following table summarizes the contributions of the selected technologies to the project:

Table 4: Evaluation of Technology Choices

| **Technology** | **Contribution to the Project** | **Advantage Over Alternative** |
| --- | --- | --- |
| Unity | Rich XR support and broad ecosystem | Lower barrier to entry and C# support than Unreal Engine |
| VIROO | Multi-user support and content management | Faster integration and ready-made infrastructure than custom solutions |
| HTC Vive Business | Quality passthrough and ergonomic design | Meta Quest does not support passthrough. |
| Zenject | Loosely coupled modular architecture | More comprehensive features than Unity's built-in DI system |
| XML Serialization | Flexible and readable data format | More structured and hierarchical data support than JSON |
| Nextcloud | Centralized content management | More flexible and customizable than commercial alternatives |

The integration of these technologies supports the main goal of the XR4MCR project, which is to create a platform that enables the creation and execution of industrial robot maintenance training scenarios without requiring code writing.

### Development Environment

The XR4MCR project has been created using various development tools, processes, and methodologies. This section describes in detail the software development environment, the tools used, and the development approach followed.

#### Development Tools and Environments

The key tools and environments used in the development of the XR4MCR project are:

##### IDE and Code Editing Tools

| **Tool** | **Purpose** | **Version** |
| --- | --- | --- |
| Visual Studio 2022 | Main C# development environment | Professional 2022 |
| Unity Editor | XR application development and testing | 2022.3.40f |
| Unity Hub | Management of Unity releases and projects | 3.11.1 |

##### Version Control and Documentation Tools

| **Tool** | **Purpose** | **Notes** |
| --- | --- | --- |
| Subversion (SVN) | Source code versioning | TortoiseSVN client in use |
| DOORS | Requirements management and traceability | For compliance with IEEE 12207 and AQAP-160 standards, requirements tracking and management |
| Teams | Project documentation and information sharing | For communication within the team |
| Redmine | Task tracking and project management | Supporting Agile processes |

#### Model and Content Creation Tools

| **Tool** | **Purpose** | **Workflow Integration** |
| --- | --- | --- |
| Blender | 3D model editing and optimization | Export to Unity with FBX format |
| FreeCAD | CAD Conversion | Conversion of CAD files to FBX/OBJ model files |
| Adobe Photoshop | Prepare textures and UI elements | Transfer with PSD/PNG formats |
| DOTween | Animation and transition effects | Code integration on Unity |

#### Test Environment and Tools

During the XR4MCR development process, a variety of environments and tools are used to test the different phases of the application:

![image](https://github.com/user-attachments/assets/dbfa43f7-1120-47aa-8f73-7e473630e3db)

Figure 7: XR4MCR Test & Deployment Flow

1. Mixed Reality Test Rigs

| Hardware | Purpose | Technical Specifications |
| --- | --- | --- |
| HTC VIVE XR Elite | Mixed reality and VR test environment | 4K resolution, 110° FOV, Passthrough camera |
| Workstation | Development and testing | Intel i9, 64GB RAM, NVIDIA RTX 4080 |
| Test Area | Physical movement and interaction tests | 3m x 4m open area, tracking sensors |

#### Development Workflow and Methodology

The XR4MCR project is being developed with a methodology that follows agile development principles and industry standards.

##### Software Development Life Cycle

The XR4MCR software development lifecycle consists of the following phases in accordance with LTG's PRS 08 Software Lifecycle Procedure and the IEEE 12207 standard:

![image](https://github.com/user-attachments/assets/c049fc27-d453-4ae1-bc5a-24e559626c43)

Figure 8: XR4MCR Software Development Life Cycle

##### Development Sprint Structure

The project is being developed in 2-week sprint cycles. Each sprint includes:

1. Sprint Planning: Setting goals and assigning tasks
2. Daily Meetings: Daily progress and obstacle assessment among team members
3. Sprint Review: Demo and review of completed work
4. Sprint Retrospective: Lessons learned for process improvement and the next sprint

##### Code Style Standards

XR4MCR code style examples:
```
// PascalCase class and method names

public class NodePresenter

{

 // camelCase variable names

 private bool \_isActive;



 // XML documentation comments

 /// &lt;summary&gt;

 /// Activates the node and triggers related events.

 /// &lt;/summary&gt;

 /// &lt;param name="silently"&gt;Set to true to activate without triggering events&lt;/param&gt;

 public void ActivateNode(bool silently = false)

 {

     // Method body

 }

}
```
Figure 9: XR4MCR Code Style Example

##### Code Review Process

The quality control process follows a structured code review flow on SVN:

1. The developer develops code based on assigned tasks
2. Code is subjected to basic testing before being committed to SVN
3. Code review is conducted by the team leader or designated reviewers
4. Issues found are corrected and reviewed again
5. Approved code is merged into the main branch

##### Documentation Standards

The XR4MCR project is documented in accordance with LTG's TLM 08 Software Development Documentation Instruction. The basic documentation standards are:

1. Requirements documents in IEEE 12207 format
2. Design documentation with UML diagrams
3. Code documentation with XML documentation comments
4. Test cases and test reports
5. User manuals and technical reference documents

##### VIROO Development Environment Integration

XR4MCR is designed to operate in a VIROO environment. Integration with the VIROO development environment is achieved through the following steps:

1. The Unity project is developed using the VIROO SDK
2. VIROO user profile and settings are integrated into the project
3. The project is compiled to be compatible with the VIROO ecosystem
4. The compiled application is deployed and managed through the VIROO Portal

The XR4MCR development environment has been designed with a quality-focused approach, following industrial standards. The tools, processes, and methodologies used ensure that the implementation is of high quality, sustainable, and extensible.

## MVP Architecture

The Model-View-Presenter (MVP) design pattern forms the basic architectural approach of the XR4MCR project. This section discusses how the MVP pattern is implemented in the project, the responsibilities of each layer, and the advantages provided by this architecture.

**Advantages of MVP Architecture and Reasons for Selection**

The main reasons for choosing MVP architecture for the XR4MCR project are:

1. Modularity and Ease of Maintenance:

• Each layer has specific responsibilities

• Changes in one layer have minimal impact on other layers

• Adding new features becomes easier

1. Testability:

• The model layer can be tested independently

• The Presenter layer can be tested by mocking dependencies

• Tests isolated from Unity dependencies can be written

1. Compatibility with Unity:

• Naturally integrates with Unity's GameObject and Component system

• The View layer directly leverages Unity's UI system

• MVP is a pattern that aligns with Unity's event-based nature

1. Multi-User Environment Support:

• The model layer can be synchronized over the network

• The Presenter layer can reflect remote changes to the local view

• Different users can easily view the same model

1. Mixed Reality Suitability:

• XR interactions are managed in the presenter layer

• The model is independent of the mixed reality environment

• View adaptation for different XR devices becomes easier

![image](https://github.com/user-attachments/assets/1e6330d6-348b-47a2-8027-caf51e4a4f4b)


Figure 10: MVP Architecture Overview

**Model Serialization and Saving/Loading**

In the XR4MCR, models are serialized, saved and uploaded in XML format. This functionality is an important feature of the model layer and ensures that scenarios can be shared.

Reasons for choosing the XML format:

- Human readability
- Conformance to hierarchical data structures
- Unity and .NET's powerful XML support
- Ease of ensuring backward compatibility in future releases

Thanks to this model structure, scenarios created by instructors can be easily recorded, shared, and uploaded in different environments. More detailed information on model serialization and data management will be provided in the 5.7 section. Node system architecture will be 5.3. Node System Architecture" section.

Modularity and Extensibility

One of the most important features of the XR4MCR architecture is its modular structure. New node types can be easily added without disturbing the existing structure:

1. A new Model class is created (derived from BaseNode)
2. The Presenter class corresponding to this model is created
3. The required Unity prefab is created for Presenter
4. A new node type is added to the factory class

Thanks to this modular structure, the scope of the system can be expanded by developing specialized node types for different areas of robotics. The MVP architecture ensures that the XR4MCR project is maintainable, testable, and extensible, supporting multi-user training scenarios in a mixed reality environment.

### Model Layer (Data and Business Logic)

The model layer contains XR4MCR's data structures and core business logic. This layer consists of components that manage the state of the application and perform data operations.

![image](https://github.com/user-attachments/assets/f23a225c-512f-429e-976e-2db4fd92d29e)

Figure 11: Model Layer Primitives

Basic Model Classes

- **BaseNode: The base class from which all node types are derived**
- **Port: Class that represents the connection points between nodes**
- **Connection: Class that represents the connection between two ports**
- **NodeGraph: The main collection that contains all nodes and connections**

Features of the Model Layer

1. **Independence**: Model classes are designed as pure C# classes without any UI or Unity dependencies.
2. **Serializability**: All model objects can be serialized and reloaded in XML format. This feature is critical for saving and sharing scenarios.
```
// Example of serializability of model classes \[Serializable\]
 public class BaseNode

{

    public string ID { get; set; }

    public string Title { get; set; }

    public Vector2 Position { get; set; }

    // Other features...

}

1. Verifiable Data: The model contains data validation logic and provides a consistent state.

// Example of port connection compatibility verification

public bool CanConnectTo(Port other)

{

    // Polarity control

    if (this.Polarity == PolarityType.Input && other.Polarity != PolarityType.Output)

        return false;



    if (this.Polarity == PolarityType.Output && other.Polarity != PolarityType.Input)

        return false;



    // Loop control

    if (CheckForLoopCreation(this, other))

        return false;



    return true;

}
```
Why Model Layer?

Separating the model layer provides the following benefits:

- **Testability**: Business logic is decoupled from visual representation, facilitating unit testing
- **Reusability**: Models can be used with different interfaces
- **Ease of Maintenance**: Changes to the data structure are isolated from UI changes

### View Layer (Unity UI Components)

In XR4MCR, the View layer is based on Unity's UI and GameObject system. This layer contains all the visual elements that the user sees and interacts with.

![image](https://github.com/user-attachments/assets/934924e3-1fbb-427f-b5f5-45fd94334696)

Figure 12: View Layer Components

View Layer Components

- **Canvas Systems:** Editor, object loading, and information canvases
- **UI Elements:** Buttons, panels, input fields, dropdown menus
- **Prefabs:** Representative objects of nodes, connections, and other visual elements
- **XR Interaction Components:** Components required for interaction in a mixed reality environment

Features of the View Layer

1. **Visual Representation:** Visually reflects the status of the model to the user.
2. **User Interaction:** Contains components that allow the user to input to the system.
3. **Responsiveness:** Reacts to user actions and model changes.
```
// Example: Unity UI Button function

\[SerializeField\] private Button \_saveButton;



void Start()

{

    \_saveButton.onClick.AddListener(() => {

        // Call the method on the Presenter layer

        \_graphPresenter.SaveGraph();

    });

}
```
### Presenter Layer (UI-Model Mediation)

The Presenter layer is the bridge between the Model and the View. It processes user interactions, updates model data, and reflects model changes to the visual interface.

![image](https://github.com/user-attachments/assets/e862d93b-0026-439a-a8e1-11b0eb0807d5)


Figure 13: Presenter Layer Hierarchy

**Presenter Layer Components**

- **BaseNodePresenter:** The base class of all node presenters
- **PortPresenter:** Class that manages the visual representation of port models
- **ConnectionPresenter:** Class that manages the visual representation of connections
- **GraphManager:** The class that manages the editor canvas and all node interactions
- **ScenarioManager:** The class that controls the scenario execution flow.

**Features of the Presenter Layer**

1. **Model-View Synchronization:** Reflects model changes to the UI and UI interactions to the model.
```
// Example of updating the node location

public void UpdateNodePosition(Vector2 newPosition)

{

    // Update the model first

    Model.Position = newPosition;



    // Then update the visual representation

    RectTransform.anchoredPosition = newPosition;

}
```
1. State Management: Manages the application state and coordinates transitions.
```
// Scenario initialization example

public void StartScenario()

{

    // Reset the state of all nodes

    ResetAllNodeStates();



    // Make your startup node active

    StartNode.ActivateNode();

    ActiveNodePresenter = StartNode;



    // Update UI

    UpdateUIForActiveNode();

}
```
1. Event Handling: Processes user interactions and system events.

### Zenject Dependency Injection

####  The Concept of Dependency Injection

Dependency Injection is a design pattern in which software components are "injected" from the outside, rather than directly referencing each other. This approach reduces tight coupling between components, creating an architecture that is more modular, testable, and easy to maintain.

![image](https://github.com/user-attachments/assets/3ebdd829-0e0b-4060-8965-39d24f73d9ea)

Figure 14:  Dependency Injection Concept

#### What is Zenject?

Zenject (currently also known as Extenject) is a powerful dependency injection framework developed for Unity. Zenject makes it easy to manage dependencies between components, control the lifecycles of objects, and write testable code.

#### Using Zenject in XR4MCR

##### Dependency Architecture

In XR4MCR, Zenject is used to manage the relationships between system components. The following diagram illustrates the basic dependency relationships in the project:
![image](https://github.com/user-attachments/assets/b01c549d-5cb6-447c-9ac9-429a4be8938c)

Figure 15: XR4MCR Zenject Dependency Architecture

##### Installer Configuration

In XR4MCR, dependencies are configured via the GraphSceneInstaller class. This installer registers the key components of the project in the Zenject container.

![image](https://github.com/user-attachments/assets/9ccd68fc-325d-41a4-b290-82ba3f9c468b)


Figure 16: Zenject Installer Configuration

#### Zenject Key Components in XR4MCR

Table 5: Zenject Component Types in XR4MCR

| **Component Type** | **Explanation** | **Examples** |
| --- | --- | --- |
| Singleton Services | One-of-a-kind services across the entire application | NodeConfig, SystemManager, Raycaster |
| Factories | Factories that enable dynamic creation of objects | NodePresenterFactory, ConnectionPresenterFactory, ObjectFactory |
| Connected from the hierarchy | Linking existing components in the Unity scene | GraphManager, UIManager, ScenarioManager, XRInputManager |
| Prefab Examples | Objects sampled from prefabs | Pointer, LTGLineRenderer |

#### Dependency Injection Methods

The three main dependency injection methods used in XR4MCR:

![image](https://github.com/user-attachments/assets/daff0a48-2327-4a20-bb19-86eeb355d03b)

Figure 17: Dependency Injection Methods

####  Zenject's Benefits to XR4MCR

Table 6: Zenject's Benefits to the XR4MCR Project

| **Advantage** | **Explanation** | **Impact on XR4MCR** |
| --- | --- | --- |
| Loosely Coupled Architecture | Direct dependencies between components are reduced | Node system, UI system and XR interaction system can be developed independently |
| Testability | Dependencies can be easily mocked | Presenter classes can be tested without Unity dependencies |
| Organizing the Code | Dependencies are centrally managed | GraphSceneInstaller configures all dependencies in one place |
| Flexible Object Creation | Objects can be created dynamically and as needed | Factory classes dynamically create nodes and connections |
| Lifecycle Management | The life cycles of objects can be controlled | The system startup sequence and cleanup processes are managed regularly |

#### Installer Example and Explanation

![image](https://github.com/user-attachments/assets/b6bc441e-4e82-4822-a131-30cbf133cc93)

Figure 18: Zenject Installer Binding Methods

#### Zenject Integration Process

Zenject integration into the XR4MCR project was accomplished through the following steps:

1. **Package Installation:** The Zenject package has been added to the project via Unity Package Manager.
2. **Creating an Installer:** By creating the GraphSceneInstaller class, the dependency configuration has been made.
3. **Context Configuration:** By adding the SceneContext component to the scene, installers are connected to this context.
4. **Dependency Injection Application:** All services and components are marked with \[Inject\] attributes for injection.
5. **Factory Patterns:** Factory classes are defined for dynamic object creation needs.
![image](https://github.com/user-attachments/assets/2850bb18-9125-4e82-9312-b668a703bcad)

Figure 19: Zenject Integration Process

#### Scopes and Their Use in XR4MCR

In Zenject, the scopes of objects determine their lifecycle:

Table 7: Zenject Scopes and Their Applications in XR4MCR

| Scope | Explanation | Usage on XR4MCR |
| --- | --- | --- |
| Singleton | A single instance throughout the entire application | Managers (GraphManager, ScenarioManager) |
| Transient | A new sample with each injection | Auxiliary utility classes |
| Scene Scope | Depends on the scene lifecycle | UI components and XR interactions |

#### Test Setup Without Scanning and Examining the Hierarchy
![image](https://github.com/user-attachments/assets/245a482a-2c1b-4863-87f6-0361da596edf)

Figure 20: Live Environment vs Test Environment Setup

#### Conclusion: Zenject's Role in XR4MCR

Zenject forms the architectural backbone of the XR4MCR project and provides the following key benefits:

1. **Modularity:** Provides loose coupling between software components, allowing each module to be developed and tested independently.
2. **Lifecycle Management:** Manages the creation, initialization, and cleanup of objects in an orderly manner.
3. **Extensibility:** Allows easy integration of new features and components into the system.
4. **Testability:** Facilitates the use of mock objects instead of real objects, enabling isolated testing of units.

Zenject integration makes the complexity of XR4MCR manageable, providing a solid foundation to support creating and running multi-user training scenarios in a mixed reality environment.

## Node System Architecture

The node system is the foundation of the XR4MCR project, allowing users to visually create training scenarios without writing code. In this section, the architectural structure, components, and working principle of the node system are explained in detail.

**Advantages of Node System Architecture**

XR4MCR's node architecture provides the following key benefits:

1. **Visual Programming:** Users can create complex scenarios without writing code
2. **Extensibility:** New node types can be easily added
3. **Serializability:** Scenarios can be saved and shared in XML format
4. **Modularity:** Each node encapsulates a specific function
5. **Testability:** Nodes can be tested independently
![image](https://github.com/user-attachments/assets/84efff71-2efa-47de-b6af-51755af5f175)

Figure 21: The Value Provided by the Node System

The node system is a critical component that fulfills XR4MCR's core goal of "creating robot maintenance training scenarios without writing code." Thanks to this system, trainers can design, record, and share interactive and effective training scenarios without requiring programming knowledge.

### Node Class Hierarchy

In XR4MCR, the node system is based on a layered class hierarchy. This hierarchy ensures that different types of nodes share common characteristics while also exhibiting specialized behaviors.
![image](https://github.com/user-attachments/assets/d32aa507-3f2b-442a-8ba1-2918c9c212f7)

Figure 22: Node Types

#### BaseNode

It is the base class for all node types and includes the following properties:

Table 8:  BaseNode Features and Methods

| Feature/Method | Explanation |
| --- | --- |
| ID  | Node's unique identifier |
| Title | User-seen title |
| Description | Explanation of the node function |
| Position | Position on the canvas |
| IsActive | Whether the node is active or not |
| IsStarted | Whether the node is started or not |
| IsCompleted | Whether the node is complete or not |
| Ports | A collection of ports |
| EventPorts | Collection of event ports |
| Initialize() | Initializes the node |
| Execute() | Performs the basic function of the node |
| Complete() | Marks the node as complete |

#### Customized Node Types

XR4MCR provides a variety of node types to support different training scenario steps:

Table 9: XR4MCR Node Types and Features

| Node Tipi | Function | Special Features |
| --- | --- | --- |
| StartNode | Scenario starting point | Includes output ports only |
| FinishNode | Scenario endpoint | Includes input ports only |
| TouchNode | Touch an object | The object to be touched (TargetObjectID) |
| GrabNode | Object handling and moving | Target object (TargetObjectID) |
| LookNode | Looking at the object | The object looked (TargetObjectID) |
| LogicNode | Logical operations | OperatorType (AND, OR) |
| ActionNode | Fundamental action node | Type, TargetObjectID, ParameterName, ParameterValue |
| AudioActionNode | Enhanced actions related to sounds | DropdownItems (list of audio options) |
| ChangeMaterialAction | Material substitution | TargetObjectID, ParameterValue (material) |
| ChangePositionAction | Changing location | TargetObjectID, ParameterValue (target position) |
| ChangeRotationAction | Rotation switching | TargetObjectID, ParameterValue (target rotation) |
| ChangeScaleAction | Scale shifting | TargetObjectID, ParameterValue (target scale) |
| DescriptionActionNode | Show a text description | ParameterValue (text to display) |

### Port and Connection System

Data flow and logical relationships between nodes are provided through the port and connection system. This system manages the flow control of the scenarios.
![image](https://github.com/user-attachments/assets/4101d24e-92e1-4057-a7ce-acbb8442d43f)


Figure 23: Port and Connection System

#### Port Types

In XR4MCR, the port system is based on the concept of polarity:

Table 10: Port Polarity Types

| **Polarity Types** | **Explanation** | **Connection Rules** |
| --- | --- | --- |
| Input | Inlet port | It can only receive connections from Output ports |
| Output | Output port | Can only connect to Input ports |
| Bidirectional | Bi-directional port | Can connect in both directions |

#### Connection Behavior

Connections define the relationships between nodes and have the following characteristics:

1. Directional Flow: Connections provide one-way flow from the source port to the target port
2. Data Transport: Some connections can carry data (e.g., variable values)
3. Activation Signal: Carries a signal for nodes to activate each other
4. Visual Representation: Connections are visually represented by lines on the canvas
![image](https://github.com/user-attachments/assets/6215d88b-f470-41ba-b0d6-0500eb06d729)

Figure 24: Connection System Layered Structure

### Event System

XR4MCR's node system uses an event-based communication mechanism. This mechanism allows nodes to communicate with each other without direct connection.
![image](https://github.com/user-attachments/assets/31b4bc2e-01cf-4e84-9e6e-d9a7b3ac2a4a)

Figure 25: Event System Structure

#### Event Types

The main event types used in the node system in XR4MCR are:

Table 11: Node Event Types

| **Event Tipi** | **Triggered Time** | **Purpose** |
| --- | --- | --- |
| OnStarted | When the node starts working | Operations such as starting animations, playing sounds |
| OnCompleted | When the node is complete | Migrate to the next node |
| OnSkip | When a node is skipped | What to do in case of jump |

#### Event Emit Mechanism
![image](https://github.com/user-attachments/assets/eed363c8-0935-48f1-b8c9-c9feed2bb0ec)

Figure 26: Event Emit Example

### Creating Nodes with Factory Pattern

In XR4MCR, nodes and connections are created centrally using the Factory Pattern. This approach standardizes the object creation process and prevents code duplication.
![image](https://github.com/user-attachments/assets/c212a7f5-661d-4d8c-97bb-3b48ec8cc3a3)

Figure 27: Creating Nodes with Factory Pattern

#### How NodePresenterFactory Works

Table 12:  NodePresenterFactory Process Steps

| **No** | **Process Step** | **Explanation** |
| --- | --- | --- |
| 1   | Receiving Requests | GraphManager makes a request to create a node |
| 2   | Prefab Selection | NodePresenterFactory selects the right prefab based on node type |
| 3   | Instantiation | Prefab is sampled on stage |
| 4   | Creating a Model | An instance of the corresponding model class is created |
| 5   | Configuration | Presenter and model are configured |
| 6   | Creating a Port | Necessary ports are created according to the node type |
| 7   | Dependency Injection | Dependencies are injected with Zenject |
| 8   | Record | The created node is registered in the GraphManager |

#### How ConnectionPresenterFactory Works

Similarly, connections are created by ConnectionPresenterFactory:
![image](https://github.com/user-attachments/assets/32889b54-94e8-428e-a5f1-062aa1d796ac)

Figure 28: Connection Creation Mechanism

## UI Systems

The XR4MCR project features three main canvas systems that are specifically designed to provide users with an effective experience in a mixed reality environment. These systems perform the functions of creating training scenarios, managing objects, and displaying information.

### Advantages of UI Systems

The XR4MCR's UI systems are specifically designed to streamline the process of creating training scenarios in a mixed reality environment.

Table 13: Advantages of UI Systems

| **Advantage** | **Explanation** | **Result** |
| --- | --- | --- |
| Intuitive Interaction | XR controls that feel natural | Shortening of the learning curve |
| Modularity | Separate canvases for different functions | Focused user experience |
| Flexibility | Positionability of canvases | Personalized work environment |
| Efficiency | Access to all interfaces from a single point of view | Workflow acceleration |
| Extensibility | Ability to add new UI components | Adaptation to future requirements |

Figure 29: Advantages of UI Systems

UI systems are a critical component that shapes the core user experience of XR4MCR, supporting the goal of creating training scenarios without writing code. These systems are designed to adapt to different user profiles and usage scenarios.

### Editor Canvas

The editor canvas is the core user interface of XR4MCR, enabling the creation and editing of node-based scenarios. This canvas provides the tools for placing nodes, creating connections, and managing scenario flow on a diagrammatic plane. The editor canvas has a structure that can be dynamically updated at runtime, modified, and positioned in a mixed reality environment. Users can place, move, and connect nodes through spatial interaction with the XR controller.
![image](https://github.com/user-attachments/assets/3006cc5a-edec-41f7-b7b7-b49c506f655e)

Figure 30: Node Editor Canvas

Table 14: Main Components of the Editor's Canvas

| **Component** | **Function** | **User Interaction** |
| --- | --- | --- |
| Node Editing Area | Node placement and orchestration | Drag-and-drop, beam interaction |
| Connection System | Establishing relationships between nodes | Port selection and connection creation |
| Control Panel | Manage scenario flow | Play, stop, pause buttons |
| Tool Menu | Node types and functions | Category selection and node creation |

The editor canvas is based on Unity's WorldSpace Canvas system and implemented with the NodeEditor(WorldSpace) prefab. This approach places the canvas in a 3D environment, allowing it to be positioned in real-world coordinates and interact naturally with XR controls.

Design Principles of the Editor's Canvas

1. Spatial Organization: Natural positioning relative to the user in a mixed reality environment
2. Interaction Coherence: Consistent behavior patterns across all interactive elements
3. Visual Feedback: Visual cues and animations that validate user interactions
4. Modularity: Simplicity and clarity by using separate components for each function
![image](https://github.com/user-attachments/assets/e4a2b758-96e5-4fb4-a082-c0c5e955c93a)

Figure 31: Editor Canvas Structure and Hierarchy

### Object Loading Canvas

The Object Loading Canvas is the interface used to select 3D models to be used in training scenarios and transfer them to the editing area. This canvas allows models to be organized into categories, searched, selected, and placed in the scene.
![image](https://github.com/user-attachments/assets/69db07eb-31a5-4ed0-a483-f88120ac85d0)

Figure 32: Object Loading Canvas

Object Loading Canvas Features

- Categorized 3D model library
- Model selection with preview images
- Search and filter functions
- Drag-and-drop models to the scene
- Controls for transforming and manipulating objects

The Object Loading Canvas has a special rendering system that allows previews of 3D models. When the user selects an object, it can be previewed before it is transferred to the editing area, enabling the user to make more informed decisions when choosing.

Table 15: Object Loading Canvas Components

| Component | Function | Interaction Model |
| --- | --- | --- |
| Category Selector | Navigating between categories of objects | Button group |
| Object Grid/List | View objects in a category | Scrollable grid |
| Search Bar | Search by object name | Text input |
| Filters | Filter by attributes | Drop-down menu and checkboxes |
| Preview | 3D preview of the selected object | Rotatable 3D image |
![image](https://github.com/user-attachments/assets/6371fd15-46d4-4e77-900a-7dd625b119d7)

Figure 33: Object Loading Canvas and Components

Object Management Workflow

1. Finding objects by category selection or search
2. Examining an object with a preview
3. Selecting the object with the XR controller
4. Importing an object into an editing area
5. Positioning the object with transformation tools

This workflow enables the user to quickly and efficiently select and place the required 3D objects for training scenarios.

### Information Canvas

The Information Canvas is the interface that provides the user with information such as scenario status, active node information, and system messages. This canvas allows the user to monitor the flow of the scenario and stay informed about the system state.
![image](https://github.com/user-attachments/assets/8850d04b-ea96-43a1-b893-4e6ec1a76b4b)

Figure 34: Information Canvas

Table 16: Information Canvas Content

| **Content** | **Explanation** | **Update Trigger** |
| --- | --- | --- |
| Scenario Progression | Current step/total step information | Node exchange |
| Node Header | The title of the active node | Node activation |
| The description of Node | Description of the active node | Node activation |
| System Messages | Important warnings and information | System events |

The Information Canvas is managed by the UIManager class to display up-to-date information at runtime for the scenario. Information is automatically updated when the active node changes, scenario progress is updated, or a significant system event occurs.
![image](https://github.com/user-attachments/assets/f37cfbfd-d778-4ba5-a1b9-ba502ad473ea)

Figure 35: Information Canvas Structure

### Scenario Playback Area

The Scenario Playback Area is the core workspace where the training scenarios of the XR4MCR application are physically set up and executed. This area provides an interactive environment that allows users to create, edit, and test training scenarios.
![image](https://github.com/user-attachments/assets/4548eddc-cdbb-496d-a6fc-6eae6300250f)

Figure 36: Scenario Playback Area

#### Overview

The Scenario Playback Area is the central area where the visual and interactive components of the training scenarios are brought together. This area provides users with the following basic functionalities:

1. Placement of selected 3D objects from the Object Canvas
2. Associating logical flows in the node system with physical objects
3. Creating the visual composition of the training scenario

Table 17: Key Components of the Scenario Playback Area

| **Component** | **Function** |
| --- | --- |
| 3D Object Positioning | Positioning of objects selected from the object canvas in the space |
| Node-to-Object Association | Establishing connections of logical nodes with physical objects |
| Interaction Points | Determining the areas where the user will interact during the training |

#### Principle of Operation

The Scenario Playback Area allows instructional designers to simulate real-world tasks in a virtual environment. Users can create their training scenarios in this area by following these steps:

1. Selecting relevant 3D models from the Object Canvas
2. Placement and positioning of selected objects in the Scenario Playback Area
3. Associating logical flows created in the Node Editor with these objects
4. Identification of interactions and dependencies between objects
![image](https://github.com/user-attachments/assets/702a3377-8628-46a8-a0a5-b8c1a8b216f0)

Figure 37: Scenario Playback Area Workflow

#### Object-to-Node Association

One of the most important functions of the Scenario Playback Area is the connection between physical objects and logical nodes. This association defines the interactive aspect of the training scenario and determines the events that will be triggered when users interact with specific objects.
![image](https://github.com/user-attachments/assets/4dbbca17-7345-4311-8659-827b75d00ef2)

Figure 38: Object-Node Linking Model

Table 18: Object-Node Linking Types

| **Attribution Type** | **Explanation** | **Sample Application** |
| --- | --- | --- |
| Direct Connection | Connecting a specific object directly to a node | Pressing a button triggers a specific node |
| Group Link | Connecting multiple objects to a group of nodes | All components of a machine part affect a single logical flow |
| Conditional Connection | Contingent relationships between objects and nodes | A shard triggers the node only when it is placed in the correct position |
| Sequential Connection | Object-node relationships to be performed in a specific order | Situations in which maintenance steps must be performed in a specific order |

#### User Interaction

In the Scenario Playback Area, users can use different forms of interaction during the training design and testing phases. These interactions are designed to provide a natural and intuitive experience.

Table 19: Scenario Playback Area Interaction Methods

| **Interaction Type** | **Explanation** | **Usage Area** |
| --- | --- | --- |
| Object Placement | Positioning of objects in space | Organization of the educational environment |
| Object Manipulation | Rotation, scaling of objects | Moving objects into the correct position |
| Labeling | Adding descriptive tags to objects | Explanation of training steps |
| Simulation Control | Running, pausing the scenario | Testing the training flow |
![image](https://github.com/user-attachments/assets/492a6194-5985-4085-80d7-18e5192d19d0)


Figure 39: Scenario Playback Area User Interaction Cycle

### Mixed Reality Canvas Layout

In the XR4MCR application, the canvases are positioned to provide an optimal working experience in the user's mixed reality environment. The spatial organization of the canvases is designed to support the user's workflow.

Table 20: Canvas Positioning Strategy

| **Canvas** | **Spatial Position** | **Purpose** |
| --- | --- | --- |
| Editor's Canvas | In front of the user | Main field of study |
| Object Upload Canvas | The editor is at the top right of the canvas | Object selection and placement |
| Knowledge Canvas | At the bottom right of the editor's canvas, tap | Information display and reference |

This layout allows the user to access all canvases from a single viewpoint, enabling an efficient workflow in a mixed reality environment. The user can work with minimal head movement when switching between canvases.
![image](https://github.com/user-attachments/assets/a07ecac8-133d-417d-bda7-0007a49b0177)

Figure 40: Canvas Layout in a Mixed Reality Environment

The positioning and management of canvases is performed by the CanvasControllerXR class. This class ensures that the canvases remain in the appropriate position from the user's point of view and respond to user interactions.

## XR and Mixed Reality Integration

The XR4MCR project features a modular architecture that integrates XR technologies to support robot maintenance training in a mixed reality environment. This section describes the XR components of the project and how they are integrated.

### XR Interaction Systems

XR4MCR's interaction systems are key components that enable users to create and run scenarios in a mixed reality environment. These systems have been implemented with customized layers built on top of the Unity XR Interaction Toolkit.

Table 21: XR4MCR Interaction System Components

| **Component** | **Responsibility** | **Architectural Layer** |
| --- | --- | --- |
| XRInputManager | Processing and abstracting XR controller inputs | Presenter |
| Raycaster | Beam-based object detection and interaction | Presenter |
| InteractionHandler | Coordinating interaction events | Presenter |
| EventSystem | Manage UI interactions | View-Presenter |

The XR interaction system is designed as part of the MVP architectural pattern and is tightly integrated with other components.
![image](https://github.com/user-attachments/assets/dd95c65b-0dac-4809-8a8b-734627ecfb07)


Figure 41: XR Interaction System Architecture

#### XRInputManager

XRInputManager processes raw inputs from physical XR controllers, providing a standardized interface to other components of the application. This component abstracts the input differences between different XR devices, providing a device-independent interaction system.

Table 22: Types of Interactions Handled by XRInputManager

| **Interaction Type** | **Usage Area** | **Example Scenario** |
| --- | --- | --- |
| Ray Pointing | UI elements and remote objects | Node selection, button activation |
| Trigger Activation | Selection and validation processes | Object placement, connection |
| Grip | Object handling and manipulation | Move 3D models |
| Motion Tracking | Controller position and rotation | Transformation gizmos |

### Mixed Reality Integration

The XR4MCR project delivers a mixed reality experience using Unity's XR infrastructure. This integration enables virtual content to interact with the real world.

Table 23: Mixed Reality Integration Components

| **Component** | **Function** | **Technical Approach** |
| --- | --- | --- |
| XR Plugin Link | Hardware-software bridge | Unity XR Plugin Framework |
| Vision System | Real and virtual image merging | Camera an Render Pipeline |
| Spatial Awareness | Location alignment with the real world | XR Anchor systems |
![image](https://github.com/user-attachments/assets/e06be7e7-5ba1-464c-a5b5-92988300a536)

Figure 42: Mixed Reality Display System

### Transformation Systems

In XR4MCR, transformation systems are a special architectural component that allows users to manipulate virtual objects in an intuitive way. This system is layered in accordance with the MVP pattern.
![image](https://github.com/user-attachments/assets/1d3b11ce-2670-49dc-91a4-ac73dc963ca9)

Figure 43: Transformation Systems

Table 24: Transformation System Components

| **Component** | **Role** | **Architectural Layer** |
| --- | --- | --- |
| TransformController | Coordinating manipulation operations | Presenter |
| TransformGizmo | Providing a visual manipulation interface | View |
| AxisController | Manage axis-based transformations | Presenter |
| ObjectTransformData | Storing conversion data | Model |
![image](https://github.com/user-attachments/assets/52b4b104-71f8-4ea3-8aea-0eb7a5f45076)

Figure 44: Transformation System Architecture

#### Axis Base Transformation

The XR4MCR uses special transformation systems for the manipulation of 3D objects. These systems enable the user to move, rotate, and scale objects in an intuitive way.

Table 25: Transformation Systems and Functions

| Transcription | Function | Control Mechanism |
| --- | --- | --- |
| Position | Moving objects in 3D space | Axis pointers and beam interaction |
| Rotation | Rotate objects on the X, Y, Z axes | Circular handles and beam interaction |
| Scale | Zoom in/out of objects | Corner handles and beam interaction |

Transformation systems provide a natural interaction between the XR controller and the user, allowing the user to manipulate objects with precision.
![image](https://github.com/user-attachments/assets/72929f09-2784-44b6-a5c0-05c8d1a3e38d)


Figure 45: Axis Based Transformation System

#### Transformation Workflow
![image](https://github.com/user-attachments/assets/c30763ca-698b-406c-84c6-bf32e4b0bcbd)

Figure 46: Transformation User Workflow

XR4MCR's transformation systems enable precise positioning of objects in training scenarios, allowing realistic robot maintenance procedures to be simulated. These systems, in accordance with the MVP architecture, are divided into object data models (Model), interaction logic (Presenter), and visual manipulators (View) layers.

The integration of XR and Mixed Reality is a key component of the XR4MCR project, providing users with the ability to design and experience training scenarios without writing code.

## VIROO Integration

The XR4MCR project is integrated with the VIROO platform to deliver multi-user mixed reality experiences. VIROO is a standalone ecosystem designed for industrial training and simulation scenarios. This section describes how the XR4MCR project is integrated with the VIROO platform and the possibilities offered by this integration.

### VIROO Ecosystem and XR4MCR Integration

The VIROO ecosystem offers a comprehensive platform for the development, deployment, and operation of XR applications. The XR4MCR project was developed using the VIROO Studio SDK and configured to run on the VIROO infrastructure.
![image](https://github.com/user-attachments/assets/1cb6ab41-4aaf-4538-96e0-a1ad6f9f03f2)

Figure 47: XR4MCR-VIROO Integration Process

The VIROO Studio SDK provides the following key components within the Unity project:

- **Content Management System:** Uploading and managing 3D models and training materials
- **Multi-User Session Management:** Connecting users to the same virtual environment
- **User Authorization:** Role-based access and authentication
- **Network Synchronization:** Synchronization of object states and user interactions

### VIROO Integration in the Development Process

The XR4MCR project uses the VIROO Studio SDK during the development phase, ensuring that the application works in harmony with the VIROO ecosystem. This integration affects the project development workflow as follows:

Table 26: Integration Steps

| **#** | **Step** | **Explanation** |
| --- | --- | --- |
| 1   | VIROO SDK Integration | Adding the VIROO Studio package to the Unity project |
| 2   | Configuring the Required Components | Making the necessary settings for access to VIROO services |
| 3   | Content Management Integration | Adaptation of 3D models and training content to the VIROO content system |
| 4   | Testing and Verification | Testing the application within the VIROO ecosystem |
| 5   | Compiling and Publishing | Release of the application to the VIROO platform |

This process ensures that the XR4MCR application can utilize all the features offered by VIROO.
![image](https://github.com/user-attachments/assets/c45887b6-e8ed-4f12-b474-adf4fa679a99)

Figure 48: VIROO and XR4MCR System Architecture

### VIROO Single Player Connection

Once the XR4MCR application is completed and published to the VIROO platform, it can be run in VIROO Single Player mode. This mode allows the user to experience training scenarios independently. VIROO Single Player mode offers the following features:

- User authentication and authorization
- Access to training content
- Scenario execution and tracking
- Performance and progress recording

In Single Player mode, the XR4MCR application runs on the infrastructure provided by the VIROO platform but does not interact with other users.

### Multi-User Sessions

One of the most powerful features of the VIROO platform is its ability to deliver multi-user training experiences. Using this feature, the XR4MCR project enables collaborative robot maintenance training.

**Creating a Multi-User Session**

1. The trainer or authorized user creates a session through the VIROO Portal
2. An XR4MCR scenario is assigned to the session
3. Participating users are invited or the session code is shared
4. Users join the session through the VIROO client

**In-Session Interaction**

- Users can see and interact with each other in the virtual environment
- The instructor can guide and support the students
- All users see the status of the training scenario in real-time
- Object manipulations and interactions are reflected to all users

This multi-user structure offers an experience similar to real-world collaboration, especially in teaching complex robot maintenance procedures.

This integration enables the XR4MCR project to be positioned as a practical and effective solution in industrial training environments.
![image](https://github.com/user-attachments/assets/6277930c-2255-4b96-bca0-3a0c5f6bc33e)

Figure 49: VIROO Multi-User Training Scenario

VIROO integration transforms the XR4MCR project from a standalone application into a training platform that can be deployed, managed, and deliver multi-user experiences on an industrial scale. This integration allows the project to reach its target audience more effectively and respond to real-world training needs.

## Data Management and Serialization

The XR4MCR project uses an efficient data management system for creating, saving, and sharing training scenarios. This section discusses the serialization of scenarios in XML format and the remote access and management of 3D models.

### XML Scenario Serialization

In XR4MCR, training scenarios are serialized, saved, and loaded in XML format. This approach ensures that scenarios are human-readable, editable, and portable between different systems. XR4MCR's serialization architecture integrated manages nodes, connections, and 3D objects in the scene within a single structure.

**Scenario Serialization Architecture**
![image](https://github.com/user-attachments/assets/1fc81c59-f437-497d-965a-9bd27637cb7f)

Figure 50: Scenario Serialization and Deserialization Process

**XR4MCR Integrated Scenario Structure**

The XR4MCR serialization system manages three main data types together in a single, integrated SaveFile structure:

1. **Nodes:** All types of nodes that make up the scenario logic
2. **Connections:** Connections that define the relationships between nodes
3. **Scene Objects:** 3D models used in the scenario and their locations

This integrated approach ensures that both the logical flow and the visual arrangement of the training scenarios are fully preserved and recreated.

**XML Scenario Structure**
```
&lt;SaveFile&gt;

 &lt;Nodes&gt;

 &lt;Node Type="StartNode" ID="node1" Title="Start" PosX="100" PosY="150"&gt;

   &lt;Ports&gt;

     &lt;Port ID="port1" Type="Output" /&gt;

   &lt;/Ports&gt;

 &lt;/Node&gt;

 &lt;Node Type="ActionNode" ID="node2" Title="Robot Arm Movement" PosX="300" PosY="150"&gt;

   &lt;Parameter Name="TargetObject" Value="obj1" /&gt;

   &lt;Parameter Name="TargetPosition" Value="10,15,20" /&gt;

   &lt;Ports&gt;

     &lt;Port ID="port2" Type="Input" /&gt;

     &lt;Port ID="port3" Type="Output" /&gt;

   &lt;/Ports&gt;

 &lt;/Node&gt;

 &lt;Node Type="FinishNode" ID="node3" Title="Finish" PosX="500" PosY="150"&gt;

   &lt;Ports&gt;

     &lt;Port ID="port4" Type="Input" /&gt;

   &lt;/Ports&gt;

 &lt;/Node&gt;

 &lt;/Nodes&gt;



 &lt;Connections&gt;

 &lt;Connection SourcePortID="port1" TargetPortID="port2" /&gt;

 &lt;Connection SourcePortID="port3" TargetPortID="port4" /&gt;

 &lt;/Connections&gt;



 &lt;SceneObjects&gt;

 &lt;Object ID="obj1" Type="Model" ResourcePath="models/robot_arm.fbx"&gt;

   &lt;Transform Position="1.5,0,2.3" Rotation="0,90,0" Scale="1,1,1" /&gt;

   &lt;Properties&gt;

     &lt;Property Name="Interactable" Value="true" /&gt;

     &lt;Property Name="CollisionType" Value="mesh" /&gt;

   &lt;/Properties&gt;

 &lt;/Object&gt;

 &lt;Object ID="obj2" Type="Light" ResourcePath=""&gt;

   &lt;Transform Position="3,5,2" Rotation="45,0,0" Scale="1,1,1" /&gt;

   &lt;Properties&gt;

     &lt;Property Name="LightType" Value="Point" /&gt;

     &lt;Property Name="Intensity" Value="2.5" /&gt;

     &lt;Property Name="Color" Value="255,255,200" /&gt;

   &lt;/Properties&gt;

 &lt;/Object&gt;

 &lt;/SceneObjects&gt;



 &lt;NodeObjectLinks&gt;

 &lt;Link NodeID="node2" ObjectID="obj1" LinkType="Target" /&gt;

 &lt;/NodeObjectLinks&gt;

&lt;/SaveFile&gt;
```
Table 27: Integrated Scenario Data Types

| **Data type** | **XML Elements** | **Serialized Features** |
| --- | --- | --- |
| Nodes | \\&lt;Nodes&gt; | Type, ID, Title, Location, Custom Parameters |
| Ports | \\&lt;Ports&gt; | ID, Tip (Input/Output/Event) |
| Connections | \\&lt;Connections&gt; | SourcePortID, TargetPortID |
| SceneObjects | \\&lt;SceneObjects&gt; | Collection of 3D objects |
| Object | \\&lt;Object&gt; | ID, Type, Source Path |
| Transform | \\&lt;Transform&gt; | Position, Turn, Scale |
| Properties | \\&lt;Properties&gt; | Object-specific properties |
| NodeObjectLinks | \\&lt;NodeObjectLinks&gt; | Node-Object relationships |

**Advantages of Integrated Serialization**

1. Complete Scenario Preservation: Both the logical flow and the visual layout of the scenario are preserved completely
2. Referential Integrity: The relationships between nodes and 3D objects are fully stored
3. Portability: The scenario can be shared as a single file and run on different systems
4. Extensibility: The XML structure allows for the addition of new data types

This serialization architecture underpins the XR4MCR project's scenario creation and sharing capabilities, enabling complete recording and recreation of training content.

### 3D Model Management and Remote Download

The XR4MCR project uses an infrastructure that allows 3D models to be retrieved and managed from a central repository. For this purpose, the Nextcloud integration offers remote access to 3D models.

**Nextcloud 3D Model Repository**
![image](https://github.com/user-attachments/assets/ad4ae6de-00c1-4fdc-8948-71b35e524f48)

Figure 51: Nextcloud Model Access Structure

**Key Components of Model Access System**

1. **Nextcloud Server:** The platform where 3D models are centrally stored and managed
2. **WebDAV Client:** The component that allows the XR4MCR application to communicate with Nextcloud
3. **Model Catalog UI:** Custom canvas where the user can view and select remote models
4. **Download Manager:** A system for downloading, tracking progress, and caching of selected models
5. **Model Loader:** The component that allows the downloaded models to be loaded into the scene in the runtime

**Model Download and Use Workflow**
![image](https://github.com/user-attachments/assets/17d467fc-75e9-4450-b970-a9014c541872)


Figure 52: Model Download and Use Process

**Remote Model Catalog Interface**

XR4MCR provides the user with a custom canvas where they can view the 3D models available on the Nextcloud server. On this canvas:

1. 3D models are shown with preview images
2. Models are listed in categorized form
3. The user can search and filter models
4. The download can be initiated with a single click
5. Download progress and speed can be viewed

**Download and Caching**

Downloaded models are cached in the "DownloadedAssets" folder in the application data directory. This allows:

1. The same model does not need to be downloaded again
2. Offline use becomes possible
3. Downloaded models are organized into subfolders according to their categories
4. Cache size is monitored and cleared as needed

**Runtime Model Loading**

XR4MCR loads downloaded 3D models into the scene at runtime using Unity's Addressable Assets system. This approach:

1. Keeps the initial size of the application small
2. Enables dynamic loading of needed models
3. Makes it possible for models to be updatable
4. Supports different model formats (.fbx, .obj, etc.)

**Placing in the Scenario Playback Area**

Models that are downloaded and added to the object canvas can be placed in the scenario area by the user:

1. The user selects a model from the object canvas
2. The selected model is positioned in the scenario area using the XR ray pointer
3. When placed in the scenario area, the model is included in the XML serialization system
4. The placed model can be associated with nodes

This model management system enables XR4MCR to use rich and diverse 3D content in training scenarios. Thanks to the central Nextcloud repository, instructors and students can easily access new content and enrich their scenarios.

