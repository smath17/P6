import unittest


class MyTestCase(unittest.TestCase):
    def test_port(self):
        port = 6000
        self.assertEqual(port, 6000)

    def test_something(self):
        writeNewTest = "here"


if __name__ == '__main__':
    unittest.main()