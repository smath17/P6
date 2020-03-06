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

# System arguments
Offline coach/Trainer: -trainer

Online coach: -coach

Keyboard controls: -k

Simple auto-score: -simplescore
