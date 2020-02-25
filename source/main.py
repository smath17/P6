import socket
import keyboard

from PlayerController import Player

teamname1 = "Bobbers"
playerCount = 11
currentPlayer = 0

# Create a list of players
team1 = [Player(teamname1) for i in range(playerCount)]

# Initially move all players from team1 onto the field
y = -30
for player in team1:
    player.send_action("(move -20 {})".format(y))  # This is a string formatted to include y in the {}
    y = y + 5

byteDash = str.encode("(dash 100)")
byteKick = str.encode("(kick 100 0)")

bufferSize = 1024


while True:
    try:  # used try so that if user pressed other than the given key error will not be shown
        if keyboard.is_pressed('x'):
            currentPlayer = currentPlayer + 1
            if currentPlayer >= playerCount:
                currentPlayer = 0

            print("current player is " + (currentPlayer + 1))
            continue
        if keyboard.is_pressed('w') or keyboard.is_pressed("up"):
            team1[currentPlayer].send_action("dash 100")
            continue
        if keyboard.is_pressed('a') or keyboard.is_pressed("down"):
            team1[currentPlayer].send_action("(dash 100 -90)")
            continue
        if keyboard.is_pressed('d'):
            team1[currentPlayer].send_action("(dash 100 90)")
            continue
        if keyboard.is_pressed('s'):
            team1[currentPlayer].send_action("(dash 100 180)")
            continue
        if keyboard.is_pressed('space'):
            team1[currentPlayer].send_action("(kick 100 0)")
            continue
        if keyboard.is_pressed('ctrl'):
            team1[currentPlayer].send_action("(catch 0)")
            continue
        if keyboard.is_pressed('shift'):
            team1[currentPlayer].send_action("(tackle 0)")
            continue
        if keyboard.is_pressed('left'):
            team1[currentPlayer].send_action("(turn -20)")
            continue
        if keyboard.is_pressed('right'):
            team1[currentPlayer].send_action("(turn 20)")
            continue
    except:
        continue

#   for player in team1:
#       player.send_action("(dash 100)")

    msgFromServer = team1[currentPlayer].rec_msg()

    msg = "Message from Server {}".format(msgFromServer)

    print(msg)

