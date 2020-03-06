import keyboard

class KeyboardControl:
    def __init__(self):

        self.player_count = 11
        self.current_player = 0

        keyboard.on_press_key('q', self.previous_player)
        keyboard.on_press_key('e', self.next_player)

    # Methods that switch between previous or next player
    # Both need an extra unused argument due to the library
    def previous_player(self, arg):
        self.current_player = self.current_player - 1
        if self.current_player < 0:
            self.current_player = self.player_count - 1

        print("current player is {}".format(self.current_player + 1))

    def next_player(self, arg):
        self.current_player = self.current_player + 1
        if self.current_player >= self.player_count:
            self.current_player = 0

        print("current player is {}".format(self.current_player + 1))

    def simple_auto_score(self, team):
        while True:
            msgFromServer = team[self.current_player].rec_msg()

            # Two if's used for Auto-Move towards and Auto-Kick ball
            if "(ball)" in msgFromServer:
                str_ball = (msgFromServer[msgFromServer.index("ball") + 6:])
                str_ball = str_ball.rsplit(" ")[0]
                if float(str_ball) < 1:
                    team[self.current_player].send_action("(kick 100 0)")

            if "(ball)" in msgFromServer:
                str_ball = (msgFromServer[msgFromServer.index("ball") + 6:])
                str_ball = str_ball.replace(")", "")
                str_ball = str_ball.rsplit(" ")[1]
                if -0.5 < int(str_ball) < 0.5:
                    team[self.current_player].send_action("(dash 60)")
                elif int(str_ball) < -0.5:
                    team[self.current_player].send_action("(dash 60 -90)")
                elif int(str_ball) > 0.5:
                    team[self.current_player].send_action("(dash 60 90)")

    def keymap(self, team):
        while True:
            try:  # used try so that if user pressed other than the given key error will not be shown
                if keyboard.is_pressed('w') or keyboard.is_pressed("up"):
                    team[self.current_player].send_action("(dash 100)")
                    continue
                if keyboard.is_pressed('a'):
                    team[self.current_player].send_action("(dash 100 -90)")
                    continue
                if keyboard.is_pressed('d'):
                    team[self.current_player].send_action("(dash 100 90)")
                    continue
                if keyboard.is_pressed('s') or keyboard.is_pressed("down"):
                    team[self.current_player].send_action("(dash 100 180)")
                    continue
                if keyboard.is_pressed('space'):
                    team[self.current_player].send_action("(kick 100 0)")
                    continue
                if keyboard.is_pressed('ctrl'):
                    team[self.current_player].send_action("(catch 0)")
                    continue
                if keyboard.is_pressed('shift'):
                    team[self.current_player].send_action("(tackle 100)")
                    continue
                if keyboard.is_pressed('left'):
                    team[self.current_player].send_action("(turn -20)")
                    continue
                if keyboard.is_pressed('right'):
                    team[self.current_player].send_action("(turn 20)")
                    continue
                if keyboard.is_pressed('1'):
                    current_player = 0
                    continue
                if keyboard.is_pressed('2'):
                    current_player = 1
                    continue
                if keyboard.is_pressed('3'):
                    current_player = 2
                    continue
                if keyboard.is_pressed('4'):
                    current_player = 3
                    continue
                if keyboard.is_pressed('5'):
                    current_player = 4
                    continue
                if keyboard.is_pressed('6'):
                    current_player = 5
                    continue
                if keyboard.is_pressed('7'):
                    current_player = 6
                    continue
                if keyboard.is_pressed('8'):
                    current_player = 7
                    continue
                if keyboard.is_pressed('9'):
                    current_player = 8
                    continue
                if keyboard.is_pressed('0'):
                    current_player = 9
                    continue
                if keyboard.is_pressed('+'):
                    current_player = 10
                    continue
                if keyboard.is_pressed('t'):
                    team[self.current_player].send_action("(say \"Hi Bob\")")
                    continue
                if keyboard.is_pressed(','):
                    team[self.current_player].send_action("turn_neck -90")
                    continue
                if keyboard.is_pressed('.'):
                    team[self.current_player].send_action("turn_neck 90")
                    continue

            except:
                continue
