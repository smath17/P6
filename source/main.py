from PlayerController import Player
from CoachController import Coach
from KeyboardControls import KeyboardControl
import sys

if __name__ == "__main__":

    teamname1 = "Bobbers"

    # Create a list of players
    team1 = [Player(teamname1, True)]
    for x in range(10):
        team1.append(Player(teamname1))



    # Initially move all players from team1 onto the field
    y = -30

    for player in team1:
        player.send_action("(move -20 {})".format(y))  # This is a string formatted to include y in the {}
        y = y + 5

    # Check for args, argv[0] is the script
    # TODO: improve parameters (multiple)
    if len(sys.argv) > 1:
        if sys.argv[1] == "-trainer":
            coach = Coach(teamname1, True)
        elif sys.argv[1] == "-coach":
            coach = Coach(teamname1)
        elif sys.argv[1] == "-k":
            controller = KeyboardControl()
            controller.keymap(team1)
        elif sys.argv[1] == "-simplescore":
            controller = KeyboardControl()
            controller.simple_auto_score(team1)

    while True:
        team1[0].update_info()
