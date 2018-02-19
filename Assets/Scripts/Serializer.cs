using System;
using System.IO;
using UnityEngine;

public class Serializer {
	public static GameLevel LoadTemporaryLevel(LevelController levelController) {
		GameLevel gameLevel = new GameLevel();
		BinaryReader br;
		
		try {
			br = new BinaryReader(new FileStream("level.tmp", FileMode.Open));
		} catch (IOException e) {
			Debug.Log(e.Message + "\nCouldn't open level.tmp!");
			return gameLevel;
		}

		gameLevel.startJunction = br.ReadInt32();
		int junctionCount = br.ReadInt32();
		for(int i = 0; i < junctionCount; i++) {
			float x = br.ReadSingle();
			float y = br.ReadSingle();
			gameLevel.Junctions.Add(
				levelController.addJunction(new Vector3(x, y, 0)));
		}

		int wireCount = br.ReadInt32();
		for(int i = 0; i < wireCount; i++) {
			Junction startNode = gameLevel.Junctions[br.ReadInt32()];
			Junction endNode = gameLevel.Junctions[br.ReadInt32()];
			gameLevel.Wires.Add(
				levelController.hookup(startNode, endNode, WireType.Plain));
		}

		br.Close();

		return gameLevel;
	}

	public static void SaveTemporaryLevel(GameLevel gameLevel) {
		BinaryWriter bw;

		try {
			bw = new BinaryWriter(new FileStream("level.tmp", FileMode.Create));
		} catch (IOException e) {
			Debug.Log(e.Message + "\nCouldn't write to level.tmp!");
			return;
		}

		bw.Write(gameLevel.startJunction);
		bw.Write(gameLevel.Junctions.Count);
		foreach (var junction in gameLevel.Junctions) {
			bw.Write(junction.transform.position.x);
			bw.Write(junction.transform.position.y);
		}
		bw.Write(gameLevel.Wires.Count);
		foreach (var wire in gameLevel.Wires) {
			bw.Write(gameLevel.Junctions.FindIndex( w => w == wire.startNode ));
			bw.Write(gameLevel.Junctions.FindIndex( w => w == wire.endNode ));
		}

		bw.Close();
	}
}
