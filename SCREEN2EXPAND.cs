using System;
using System.IO;


class SCREEN2EXPAND
{
	static void Main()
	{

		DirectoryInfo DI = new DirectoryInfo (".");
		FileInfo [] monochromeFI = DI.GetFiles("*.bmp");
		
		foreach (FileInfo monochrome in monochromeFI)
		{
		
		FileStream monochromeFS = new FileStream(monochrome.Name, FileMode.Open, FileAccess.Read);
		BinaryReader monochromeBR = new BinaryReader(monochromeFS);

		monochromeFS.Seek(18,SeekOrigin.Begin);	//Find information about monochrome picture width & height.

		Int32 monochromeWidth = monochromeBR.ReadInt32();	//Save Width value.
		Int32 colorWidth = monochromeWidth/3;			//Width of color image is 3 times narrower.
		Int32 colorHeight = monochromeBR.ReadInt32();		//Height for color bit map header will be the same.
		
	
		monochromeFS.Seek(28,SeekOrigin.Begin);
		UInt16 bitsperpixel = monochromeBR.ReadUInt16();	//Save BitsPerPixel value.
		monochromeBR.Close();
		monochromeFS.Close();
	

		if ((monochromeWidth%96f)>0)				//Width of monochrome image must be divisible by 96 pixels.
			{						//Check incoming width compliance, otherwise skip a file.
			Console.WriteLine("The WIDTH of incoming '"+monochrome.Name+"' file");
			Console.WriteLine("is not completely divided by 96 pixels.");
			Console.WriteLine("Skipping...");				
			}												
		else if (bitsperpixel!=1)				//The incoming file must be 1 bit per pixel image.
			{						//Check incoming bits per pixel compliance, otherwise skip a file.
			Console.WriteLine("The incoming '"+monochrome.Name+"' is not 1 bit per pixel file.");
			Console.WriteLine("Skipping...");
			}
		else
			{

		byte [] monochromeArray = File.ReadAllBytes(monochrome.Name);		 //Read monochrome bit map file.
		
		byte [] colorArray = new byte [(((monochromeArray.Length-62)/3)*4)+118]; //Cut 62 bytes monochrome file header.
											//Get data proportion monocrome-3:4-color.
											//Add 118 bytes of color file header.
		
		int colorArrayIndex=118; //Set offset of color array to the image data area. It will be filled with converted data.

		for (int z=62; z<(monochromeArray.Length); z+=3)			 //OnTheFly Conversion.
		{	
		int firstpixel = ((monochromeArray [z] & (1<<7))+(monochromeArray [z] & (1<<6))+(monochromeArray [z] & (1<<5)))>>5;
		int secondpixel = ((monochromeArray [z] & (1<<4))+(monochromeArray [z] & (1<<3))+(monochromeArray [z] & (1<<2)))>>2;
		int thirdpixel = ((monochromeArray [z] & (1<<1))+(monochromeArray [z] & (1<<0)))<<1;
		
		thirdpixel += (monochromeArray [z+1] & (1<<7))>>7;
		int fourthpixel = ((monochromeArray [z+1] & (1<<6))+(monochromeArray [z+1] & (1<<5))+(monochromeArray [z+1] & (1<<4)))>>4;
		int fifthpixel = ((monochromeArray [z+1] & (1<<3))+(monochromeArray [z+1] & (1<<2))+(monochromeArray [z+1] & (1<<1)))>>1;
		int sixthpixel = (monochromeArray [z+1] & (1<<0))<<2;
		
		sixthpixel += ((monochromeArray [z+2] & (1<<7))+(monochromeArray [z+2] & (1<<6)))>>6;
		int seventhpixel = ((monochromeArray [z+2] & (1<<5))+(monochromeArray [z+2] & (1<<4))+(monochromeArray [z+2] & (1<<3)))>>3;
		int eighthpixel = (monochromeArray [z+2] & (1<<2))+(monochromeArray [z+2] & (1<<1))+(monochromeArray [z+2] & (1<<0));
		
		colorArray [colorArrayIndex++] = (byte)(HighByte(firstpixel) + LowByte(secondpixel));
		colorArray [colorArrayIndex++] = (byte)(HighByte(thirdpixel) + LowByte(fourthpixel));
		colorArray [colorArrayIndex++] = (byte)(HighByte(fifthpixel) + LowByte(sixthpixel));
		colorArray [colorArrayIndex++] = (byte)(HighByte(seventhpixel) + LowByte(eighthpixel));
		}
		
		
		byte [] colorHeader = {66,77,0,0,0,0,0,0,0,0,118,0,0,0,40,0,0,0,0,0,0,0,0,0,0,0,1,0,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,128,0,0,128,0,0,0,128,128,0,128,0,0,0,128,0,128,0,128,128,0,0,128,128,128,0,192,192,192,0,0,0,255,0,0,255,0,0,0,255,255,0,255,0,0,0,255,0,255,0,255,255,0,0,255,255,255,0}; //Prepare color bit map header template.

		FileStream colorFS = new FileStream("Compressed_"+monochrome.Name, FileMode.Create);
		BinaryWriter colorBW = new BinaryWriter(colorFS);
		
		colorBW.Write(colorArray); 			//Write color bit map image data
		
		colorBW.Seek(0,SeekOrigin.Begin);
		colorBW.Write(colorHeader);			//Write color bit map header data template.
		
		colorBW.Seek(2,SeekOrigin.Begin);
		colorBW.Write(colorArray.Length);		//Put Size of result file in color bit map header. 
		
		colorBW.Seek(18,SeekOrigin.Begin);
		colorBW.Write(colorWidth);			//Put Width in color bit map header. 
		colorBW.Write(colorHeight);			//Put Height in color bit map header. 
		
		colorBW.Seek(34,SeekOrigin.Begin);
		colorBW.Write(colorArray.Length-118);		//Put Size of image data in color bit map header. 
		
		colorBW.Close();
		colorFS.Close();
		
			}
		}

	}
	
	static int HighByte (int HB)
	{
	switch (HB)
		{
		case 0: HB = 0;
		break;
		case 1: HB = 192; //C0
		break;
		case 2: HB = 160; //A0
		break;
		case 3: HB = 224; //E0
		break;
		case 4: HB = 144; //90
		break;
		case 5: HB = 208; //D0
		break;
		case 6: HB = 176; //B0
		break;
		case 7: HB = 240; //F0
		break;
		}

	return HB;
	}
	
	static int LowByte (int LB)
	{
	switch (LB)
		{
		case 0: LB = 0;
		break;
		case 1: LB = 12; //0C
		break;
		case 2: LB = 10; //0A
		break;
		case 3: LB = 14; //0E
		break;
		case 4: LB = 9; //09
		break;
		case 5: LB = 13; //0D
		break;
		case 6: LB = 11; //0B
		break;
		case 7: LB = 15; //0F
		break;
		}

	return LB;
	}
}
