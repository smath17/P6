# RoboCup 2D using Unity ML-Agents

## Opening the project

1. Install Unity 2019.3
2. Open the RoboCup-Unity folder as a project
3. Open the Robocup scene in Unity (`Assets/Scenes/Robocup.unity`)

## Important Info

After opening the project in Unity, Make sure to revert  `RoboCup-Unity/Library/PackageCache/com.unity.ml-agents@1.0.0-preview/Runtime/Agent.cs` back to the previous version that has a `GetReward()` method.

Do this either by discarding the change via your git client or by keeping a manual backup of the file.

## Running the project

After starting an instance of rcssserver (either locally or on a separate computer)

1. Select the RoboCup object in the Hierarchy
2. Choose the appropriate settings in the Inspector (such as the server's IP)
3. Press the play button

## Setup and training (only relevant when training new models)

### Create environment
open terminal in `P6/RoboCup-Unity/python-envs`
run `python -m venv your-env-name`

### Install/update mlagents
python package `pip install mlagents`
update `pip install upgrade mlagents`

### Set up terminal
open terminal in `P6/RoboCup-Unity/python-envs/your-env-name/Scripts`
run `activate`
cd to `P6/RoboCup-Unity/training`

### Training
to start training: `mlagents-learn config.yaml --run-id=bob`
to continue training, add: `--resume`

### Tensorboard
start tensorboard: `tensorboard --logdir=summaries --port=6006`
then view tensorboard here: http://localhost:6006/

## Connection to rc-server (only relevant for group members)

Connect to ssl-vpn1.aau.dk via Cisco

Might have to change firewall settings

Windows Defender Firewall --> Advanced Settings -->Inbound Rules

New Rule:

- Custom
- All Programs
- Protocol type: UDP
- Remote IP: These IP addresses: host computer's IP
- Allow the connection
- Domain/Private/Public
- Name: robocup