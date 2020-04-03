import socket
from ServerParser import Parser
import Formations


class Player:

    # goalie of optional, default is false
    def __init__(self, teamname, goalie=False, connect=True):
        # Player attributes
        self.stamina = 0
        self.speed = 0
        self.effort = 0
        self.head_angle = 0
        self.tackled = 0
        self.game_status = "before_kick_off"  # Initial mode
        self.observables = []
        # TODO: add side, grab in send_init
        self.side = 'l'  # l or r
        self.unum = None  # Uniform number

        # Instantiate parser to update info
        self.parser = Parser()

        # Set server IP
        # With is a safety wrapper
        with open("ip_address.txt", "r") as file:
            self.ip = file.read()

        if connect:
            # Server on port 6000 by default
            self.init_port = 6000
            self.serverAddressPort = (self.ip, self.init_port)

            # Create client via UDP socket
            self.UDPClientSocket = socket.socket(family=socket.AF_INET, type=socket.SOCK_DGRAM)

            # Initialize player on team
            if not goalie:
                self.initString = "(init " + teamname + " (version 16)" + ")"
            else:
                self.initString = "(init " + teamname + " (version 16) (goalie))"

            self.send_init(self.initString)

            # Update unum then get formation
            self.update_info()
            self.std_pos = Formations.standard_formation(self.unum)
            self.gchar_pos = Formations.g_formation(self.unum)

            # Set initial formation
            self.send_action('(move {} {})'.format(self.std_pos[0], self.std_pos[1]))

    # Wrapper function for UDP communication + byte encoding
    def send_init(self, action):
        self.send_action(action)
        # Receive server init output and grab dedicated port
        init_msg = self.UDPClientSocket.recvfrom(6000)
        # new port = (ip, port)
        new_port = init_msg[1]
        self.serverAddressPort = (self.ip, new_port[1])
        self.parser.init_info(self, init_msg[0].decode())

    def send_action(self, action):
        # action is null terminated because server is written in c++
        self.UDPClientSocket.sendto(str.encode(action + '\0'), self.serverAddressPort)

    def rec_msg(self):
        # Receive message from server, decode from bytes
        msg_from_server = self.UDPClientSocket.recvfrom(6000)

        # the received msg is (bytes, encoding) we just want the bytes, hence [0]
        return msg_from_server[0].decode()

    # Parse info from server, returns list of observables
    def update_info(self):
        self.observables = self.parser.parse_info(self.rec_msg(), self)

    def disconnect(self):
        self.send_action("(bye)")

    def stop_connection(self):
        self.UDPClientSocket.close()

    def formation_change(self):
        # if player may move, move
        accept_strings = {'before_kick_off', 'goal_r_', 'goal_l_'}

        if self.game_status == 'goal_{}_'.format(self.side):
            # BM time
            self.send_action('(move {} {})'.format(self.gchar_pos[0], self.gchar_pos[1]))
            # Will reset to normal after
        if self.game_status in accept_strings:
            # Reset field
            self.send_action('(move {} {})'.format(self.std_pos[0], self.std_pos[1]))
