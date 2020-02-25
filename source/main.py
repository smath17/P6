import socket
import keyboard

from PlayerController import Player

teamname1 = "Bobbers"
player_count = 11
current_player = 0


def previous_player():
    global current_player
    current_player = current_player - 1
    if current_player < 0:
        current_player = player_count - 1

    print("current player is {}".format(current_player + 1))


def next_player():
    global current_player
    current_player = current_player + 1
    if current_player >= player_count:
        current_player = 0

    print("current player is {}".format(current_player + 1))


if __name__ == "__main__":
    # Create a list of players
    team1 = [Player(teamname1) for i in range(player_count)]

    # Initially move all players from team1 onto the field
    y = -30
    for player in team1:
        player.send_action("(move -20 {})".format(y))  # This is a string formatted to include y in the {}
        y = y + 5

    byteDash = str.encode("(dash 100)")
    byteKick = str.encode("(kick 100 0)")

    bufferSize = 1024

    keyboard.on_press_key('q', previous_player)
    keyboard.on_press_key('e', next_player)

    while True:
        try:  # used try so that if user pressed other than the given key error will not be shown
            if keyboard.is_pressed('w') or keyboard.is_pressed("up"):
                team1[current_player].send_action("(dash 100)")
                continue
            if keyboard.is_pressed('a'):
                team1[current_player].send_action("(dash 100 -90)")
                continue
            if keyboard.is_pressed('d'):
                team1[current_player].send_action("(dash 100 90)")
                continue
            if keyboard.is_pressed('s') or keyboard.is_pressed("down"):
                team1[current_player].send_action("(dash 100 180)")
                continue
            if keyboard.is_pressed('space'):
                team1[current_player].send_action("(kick 100 0)")
                continue
            if keyboard.is_pressed('ctrl'):
                team1[current_player].send_action("(catch 0)")
                continue
            if keyboard.is_pressed('shift'):
                team1[current_player].send_action("(tackle 0)")
                continue
            if keyboard.is_pressed('left'):
                team1[current_player].send_action("(turn -20)")
                continue
            if keyboard.is_pressed('right'):
                team1[current_player].send_action("(turn 20)")
                continue
        except:
            continue

#      for player in team1:
#           player.send_action("(dash 100)")

        msgFromServer = team1[current_player].rec_msg()

        msg = "Message from Server {}".format(msgFromServer)

        print(msg)

