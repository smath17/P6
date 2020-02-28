import socket


class Player:

    def __init__(self, teamname, goalie=False):
        self.stamina = 0
        self.speed = 0
        self.head_angle = 0
        self.tackled = 0

        # local hosted server on port 6000 by default
        self.serverAddressPort = ("172.31.253.149", 6000)

        # Create client via UDP socket
        self.UDPClientSocket = socket.socket(family=socket.AF_INET, type=socket.SOCK_DGRAM)

        # Initialize player on team
        if not goalie:
            self.initString = "(init " + teamname + ")"
        else:
            self.initString = "(init " + teamname + " (goalie))"

        self.send_action(self.initString)

    # Wrapper function for UDP communication + byte encoding
    def send_action(self, action):
        self.UDPClientSocket.sendto(str.encode(action), self.serverAddressPort)

    def rec_msg(self):
        # Receive message from server, decode from bytes
        msg_from_server = self.UDPClientSocket.recvfrom(1024)
        # the received msg is (bytes, encoding) we just want the bytes, hence [0]
        return msg_from_server[0].decode()

        # Works for printing direct message
        # msg = "Message from server: {}".format(msg_from_server[0].decode())
        # print(msg)

    # str arg used for testing
    # TODO: can only be used after kick_off
    def parse_info(self, rec_msg):
        # In order to only get the required numbers, we spilt the string by spaces and load it into a list
        info_list = rec_msg.rsplit(" ")
        # the 6th element is current stamina
        self.stamina = int(info_list[6])
        self.speed = int(info_list[9])

        # TODO: If real, then load into player, if not remove
        # figure out if effort and directionOfSpeed is real
        print("Stamina: {}".format(self.stamina))
        print("Effort: {}".format(info_list[7]))
        print("direction of speed: {}".format(info_list[10]))