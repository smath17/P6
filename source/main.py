from PlayerController import Player

if __name__ == "__main__":

    teamname1 = "Bobbers"
    # Create a list of players
    team1 = [Player(teamname1) for i in range(11)]

    # Initially move all players from team1 onto the field
    y = -30
    for player in team1:
        player.send_action("(move -20 {})".format(y))  # This is a string formatted to include y in the {}
        y = y + 5

    byteDash = str.encode("(dash 100)")
    byteKick = str.encode("(kick 100 0)")

    bufferSize = 1024

    while True:
        for player in team1:
            player.send_action("(dash 100)")

        msgFromServer = team1[4].rec_msg()

        msg = "Message from Server {}".format(msgFromServer[0])

        print(msg)
