from PlayerController import Player
from CoachController import Coach
from KeyboardControls import KeyboardControl
from TeamSetup import TeamSetup
import sys


def createTeam():
    # Create a list of players
    team1 = [Player(teamname1, True)]
    for x in range(10):
        team1.append(Player(teamname1))

    # Initially move all players from team1 onto the field
    y = -30

    for player in team1:
        player.send_action("(move -20 {})".format(y))  # This is a string formatted to include y in the {}
        y = y + 5
    return team1


if __name__ == "__main__":

    try:
        ip_file = open("ip_address.txt")
    except IOError:
        print("Missing ip_address.txt")
        exit(1)

    teamname1 = "Simon"

    # Check for args, argv[0] is the script
    # TODO: improve parameters (multiple)
    if len(sys.argv) > 1:
        if sys.argv[1] == "-trainer":
            team1 = createTeam()
            coach = Coach(teamname1, True)
        elif sys.argv[1] == "-coach":
            team1 = createTeam()
            coach = Coach(teamname1)
        elif sys.argv[1] == "-k":
            team1 = createTeam()
            controller = KeyboardControl()
            controller.keymap(team1)
        elif sys.argv[1] == "-simplescore":
            team1 = createTeam()
            controller = KeyboardControl()
            controller.simple_auto_score(team1)
        elif sys.argv[1] == "-multiprocess":
            team = TeamSetup(teamname1)
        elif sys.argv[1] == "-basetrain":
            player = Player(teamname1)
            coach = Coach(teamname1, True)
            coach.goal_basic_training(player, teamname1)
