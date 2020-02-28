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

    def test_parse(self):
        test_info = "(sense_body 0 (view_mode high normal) (stamina 4000 1) (speed 0 0) (head_angle 0) (kick 0) (dash 0)" \
                      " (turn 0) (say 0) (turn_neck 0) (catch 0) (move 0) (change_view 0) (arm (movable 0) (expires 0)" \
                      " (target 0 0) (count 0)) (focus (target none) (count 0)) (tackle (expires 0) (count 0)))"
        self.player.parse_info(test_info)

        assert self.player.get_stamina() == 4000, "Stamina is wrong"
        assert self.player.speed == 0, "Speed is wrong"



if __name__ == '__main__':
    unittest.main()
