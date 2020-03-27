import socket


class Coach:
    def __init__(self, teamname, offline=False):
        self.teamname = teamname
        self.ip = '172.30.53.100'

        if offline:
            # Server needs to run with -server::coach=on, to disable the referee
            self.coach_port = 6001
            print("Requires -server::coach=on")
        else:
            self.coach_port = 6002

        # TODO. IP of the day
        self.serverAddressPort = (self.ip, self.coach_port)

        # Create client via UDP socket
        self.UDPClientSocket = socket.socket(family=socket.AF_INET, type=socket.SOCK_DGRAM)

        self.init_string = "(init " + teamname + " (version 16))"
        self.send_init(self.init_string)

    def send_init(self, action):
        self.send_action(action)
        # Receive server init output and grab dedicated port
        init_msg = self.UDPClientSocket.recvfrom(6000)
        # new port = (ip, port)
        new_port = init_msg[1]
        self.serverAddressPort = (self.ip, new_port[1])

    def send_action(self, action):
        # action is null terminated because server is written in c++
        self.UDPClientSocket.sendto(str.encode(action + '\0'), self.serverAddressPort)

    def rec_msg(self):
        # Receive message from server, decode from bytes
        msg_from_server = self.UDPClientSocket.recvfrom(6000)

        # the received msg is (bytes, encoding) we just want the bytes, hence [0]
        return msg_from_server[0].decode()

    def disconnect(self):
        self.send_action("(bye)")

    def stop_connection(self):
        self.UDPClientSocket.close()

    # Wrapper for move_object_inner, as it needs try/except
    def move_player(self, player, x, y):
        try:
            self.__move_player_func(player, x, y)
        except ValueError as err:
            print(err.args)

    # Moves the target playerNum on coach's team to x,y coord (PRIVATE)
    def __move_player_func(self, player, x, y):
        if self.coach_port != 6001:
            raise ValueError('Wrong type of coach, should be offline/trainer')
        self.send_action('(move (p "{}" {}) {} {})'.format(self.teamname, player.unum, x, y))

    def move_ball(self, x, y):
        try:
            self.__move_ball_func(x, y)
        except ValueError as err:
            print(err.args)

    # Moves the ball to target destination (PRIVATE)
    def __move_ball_func(self, x, y):
        if self.coach_port != 6001:
            raise ValueError('Wrong type of coach, should be offline/trainer')
        self.send_action('(move (b) {} {})'.format(x, y))

    def reset_ball(self):
        self.move_ball(0, 0)