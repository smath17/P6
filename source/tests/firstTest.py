import unittest

from PlayerController import Player


class MyTestCase(unittest.TestCase):

    def setUp(self):
        self.player = Player("TestTeam")

    def test_port(self):
        port = 6000
        self.assertEqual(port, 6000)

    def test_something(self):
        writeNewTest = "here"

    def test_parse_sense(self):
        test_info = "(sense_body 0 (view_mode high normal) (stamina 4000 1) (speed 0 0) (head_angle 0) (kick 0) (dash 0)" \
                    " (turn 0) (say 0) (turn_neck 0) (catch 0) (move 0) (change_view 0) (arm (movable 0) (expires 0)" \
                    " (target 0 0) (count 0)) (focus (target none) (count 0)) (tackle (expires 0) (count 0)))"
        self.player.parse_info(test_info)

        assert self.player.stamina == 4000, "Stamina is wrong"
        assert self.player.speed == 0, "Speed is wrong"

    def test_parse_see(self):
        test_info = "(see 930 ((flag c) 20.7 14 0 0) ((flag r t) 78.3 -21) ((flag r b) 82.3 28) ((flag g r b) 73.7 9)" \
                    " ((goal r) 73 3) ((flag g r t) 72.2 -1) ((flag p r b) 61.6 24) ((flag p r c) 56.3 5) ((flag p r t)" \
                    " 58 -15) ((ball) 20.1 14 0 0) ((line r) 72.2 90))"
        self.player.parse_info(test_info)

if __name__ == '__main__':
    unittest.main()
