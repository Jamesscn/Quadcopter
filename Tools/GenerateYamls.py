params = [
    [0.8, 0.9, 0.995],
    [0.9, 0.925, 0.95]
]
defaultYaml = open("ppo.yaml", "r")
defaultLines = []
for line in defaultYaml:
    defaultLines.append(line)
for a in params[0]:
    for b in params[1]:
        newYaml = open("est_%d_%d.yaml" % (1000*a, 1000*b), "w")
        for line in defaultLines:
            newLine = line.replace("$1", str(a))
            newLine = newLine.replace("$2", str(b))
            newYaml.write(newLine)
        newYaml.close()
defaultYaml.close()
