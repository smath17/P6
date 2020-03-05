import atexit
import socket


class Player:

    # goalie of optional, default is false
    def __init__(self, teamname, goalie=False):
        self.stamina = 0
        self.speed = 0
        self.effort = 0
        self.head_angle = 0
        self.tackled = 0
        self.game_status = ""

        # local hosted server on port 6000 by default
        # TODO. IP of the day
        self.init_port = 6000
        self.serverAddressPort = ("172.31.253.241", self.init_port)

        # Create client via UDP socket
        self.UDPClientSocket = socket.socket(family=socket.AF_INET, type=socket.SOCK_DGRAM)

        # Initialize player on team
        if not goalie:
            self.initString = "(init " + teamname + " (version 16)" + ")"
        else:
            self.initString = "(init " + teamname + " (version 16) (goalie))"

        self.send_init(self.initString)

    # Wrapper function for UDP communication + byte encoding
    def send_init(self, action):
        self.send_action(action)
        # Receive server init output and grab dedicated port
        new_port = self.UDPClientSocket.recvfrom(6000)[1]
        self.serverAddressPort = ("172.31.253.241", new_port[1])

    def send_action(self, action):
        # action is null terminated because server is written in c++
        self.UDPClientSocket.sendto(str.encode(action + '\0'), self.serverAddressPort)

    def rec_msg(self):
        # Receive message from server, decode from bytes
        msg_from_server = self.UDPClientSocket.recvfrom(6000)

        # the received msg is (bytes, encoding) we just want the bytes, hence [0]
        # print(msg_from_server[0].decode())
        return msg_from_server[0].decode()

    def disconnect(self):
        self.send_action("(bye)")
