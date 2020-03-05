from PlayerController import Player
from ServerParser import Parser


if __name__ == "__main__":

    teamname1 = "Simon"
    # Create a list of players
    team1 = [Player(teamname1, True)]
    for x in range(10):
        team1.append(Player(teamname1))

    parser = Parser()



    # Initially move all players from team1 onto the field
    y = -30

    for player in team1:
        player.send_action("(move -20 {})".format(y))  # This is a string formatted to include y in the {}
        y = y + 5

    while True:
        #    for player in team1:
        #        player.send_action("(dash 100)")

        #team1[0].rec_msg()
        parser.parse_info(team1[0].rec_msg(), team1[0])
    # team1[4].parse_info(team1[4].rec_msg())
