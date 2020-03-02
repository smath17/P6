from PlayerController import Player

if __name__ == "__main__":

    teamname1 = "Bobzors"
    # Create a list of players
    team1 = [Player(teamname1, True)]
    for x in range(10):
        team1.append(Player(teamname1))

    # Initially move all players from team1 onto the field
    y = -30

    for player in team1:
        player.send_action("(move -20 {})".format(y))  # This is a string formatted to include y in the {}
        y = y + 5

    while True:
        for player in team1:
            player.send_action("(dash 100)")

        #team1[4].parse_info(team1[4].rec_msg())