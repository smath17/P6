# Contains dictionaries for different formations based on the player's uniform number
# Needed as multiple objects are run simultaneously


def standard_formation(unum):
    return {
        1: [-49, 0],
        2: [-25.0, -5.0],
        3: [-25.0, 5.0],
        4: [-25.0, -10.0],
        5: [-25.0, 10.0],
        6: [-25.0, 0.0],
        7: [-15.0, -5.0],
        8: [-15.0, 5.0],
        9: [-15.0, -10.0],
        10: [-15.0, 10.0],
        11: [-15.0, 0.0]
    }[unum]


def g_formation(unum):
    return {
        1: [-50, 0],
        2: [-20, -8],
        3: [-20, 8],
        4: [-18, -18],
        5: [-18, 18],
        6: [-15, 0],
        7: [0, -12],
        8: [0, 12],
        9: [10, -22],
        10: [10, 22],
        11: [10, 0]
    }[unum]
