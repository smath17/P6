import socket


class Coach:
    def __init__(self, teamname, offline=False):
        self.teamname = teamname
        # With is a safety wrapper
        with open("ip_address.txt", "r") as file:
            self.ip = file.read()

        if offline:
            # Server needs to run with -server::coach=on, to disable the referee
            self.coach_port = 6001
            self.init_string = "(init (version 16))"
            print("Requires -server::coach=on")
        else:
            self.coach_port = 6002
            self.init_string = "(init " + teamname + " (version 16))"

        # TODO. IP of the day
        self.serverAddressPort = (self.ip, self.coach_port)

        # Create client via UDP socket
        self.UDPClientSocket = socket.socket(family=socket.AF_INET, type=socket.SOCK_DGRAM)

        self.send_init(self.init_string)

    def send_init(self, action):
        # Cannot use send_action, as we need to receive port
        self.UDPClientSocket.sendto(str.encode(action + '\0'), self.serverAddressPort)
        # Receive server init output and grab dedicated port
        init_msg = self.UDPClientSocket.recvfrom(6000)
        # new port = (ip, port)
        new_port = init_msg[1]
        self.serverAddressPort = (self.ip, new_port[1])

        # Receive 20 inits, these have to be read
        self.rec_dummy_msg()

    def rec_dummy_msg(self):
        self.send_action('(look)')
        for x in range(20):
            self.__rec_msg()

    def send_action(self, action):
        # action is null terminated because server is written in c++
        self.UDPClientSocket.sendto(str.encode(action + '\0'), self.serverAddressPort)
        # return message will have to be received
        return self.__rec_msg()

    def __rec_msg(self):
        # Receive message from server, decode from bytes
        msg_from_server = self.UDPClientSocket.recvfrom(6000)
        print(msg_from_server[0].decode())

        # the received msg is (bytes, encoding) we just want the bytes, hence [0]
        return msg_from_server[0].decode()

    def disconnect(self):
        action = "(bye)"
        self.UDPClientSocket.sendto(str.encode(action + '\0'), self.serverAddressPort)

    def stop_connection(self):
        self.UDPClientSocket.close()

    # Wrapper for move_object_inner, as it needs try/except
    def move_player(self, player, teamname, x, y):
        try:
            self.__move_player_func(player, teamname, x, y)
        except ValueError as err:
            print(err.args)

    # Moves the target playerNum on coach's team to x,y coord (PRIVATE)
    def __move_player_func(self, player, teamname, x, y):
        if self.coach_port != 6001:
            raise ValueError('Wrong type of coach, should be offline/trainer')
        self.send_action('(move (player {} {}) {} {})'.format(teamname, player.unum, x, y))

    def move_ball(self, x, y):
        try:
            self.__move_ball_func(x, y)
        except ValueError as err:
            print(err.args)

    # Moves the ball to target destination (PRIVATE)
    def __move_ball_func(self, x, y):
        if self.coach_port != 6001:
            raise ValueError('Wrong type of coach, should be offline/trainer')
        self.send_action('(move (ball) {} {})'.format(x, y))

    def reset_ball(self):
        self.move_ball(0, 0)

    def kickoff(self):
        self.send_action('(start)')

    # Reset stamina
    def recover(self):
        self.send_action('(recover)')

    # Returns server answer
    def check_ball(self):
        msg = self.send_action('(check_ball)')
        # msg = self.rec_msg()
        msg = msg.rsplit(' ')
        # Ball posistion (goal_r, goal_l, in_field, out_of_field)
        bPOS = msg[3][:-2]
        print(bPOS)
        return bPOS

    def goal_basic_training(self, teamname, player):
        # Initial setup
        self.move_player(player, teamname, 20, 0)
        self.kickoff()
        self.move_ball(30, 0)

        # TODO: should be episode based instead of true
        while True:
            if self.check_ball() == "goal_r":
                self.move_ball(30, 0)
                self.move_player(player, teamname, 20, 0)
                # Add points
