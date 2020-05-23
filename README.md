# RoboCup 2D using Unity ML-Agents


## Connection to rc-server
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

## Setup and training

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