from multiprocessing import Pool
from PlayerController import Player

# Handles creation of team and multiprocesses
class TeamSetup:
    def __init__(self, teamname):
        self.teamname = teamname

        # Start new process for each player, starmap for multiple arguments
        try:
            with Pool() as p:
                p.starmap(Player, [(teamname, True)])
                for x in range(10):
                    p.map(Player, [teamname])
        finally:
            p.close()
            p.join()

    def createPlayer(self):
        player = Player(self.teamname)
        # player.learn()

    def createGoalie(self):
        goalie = Player(self.teamname, True)
        # goalie.learn()
