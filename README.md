# P6
RoboCup-Soccer-2D


# Connection to rc-server
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

# RCSS Messages
All messages are null-terminated, which means they have a trailing '\0'.
As Python does not use this, we will have to remove it for every message received and add it for every sent

This is Important to note as it will not show in Prints or debugging, but can still be the cause of bugs
# System arguments
Offline coach/Trainer: -trainer

Online coach: -coach

Keyboard controls: -k

Simple auto-score: -simplescore
