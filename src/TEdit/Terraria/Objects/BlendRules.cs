using System;
using System.Collections.Generic;
using System.Linq;
using TEdit.Geometry.Primitives;
using TEditXna.Editor;
using TEditXna.ViewModel;
using TEditXNA.Terraria;

/* Heathtech */
namespace TEditXna.Terraria.Objects
{
    class BlendRules
    {
        private static BlendRules instance;

        private LinkedList<MatchRule>[] baseRules = new LinkedList<MatchRule>[16];
        private LinkedList<MatchRule>[] blendRules = new LinkedList<MatchRule>[16];
        private LinkedList<MatchRule>[] grassRules = new LinkedList<MatchRule>[16];
        public Random randomVariation = new Random();

        //This map is used during lazy merge validation to see whether a given tile is technically merged or not
        public byte[,] lazyMergeValidation = new byte[,] {
            {11, 13, 13, 13, 14, 10, 8, 8, 8, 1, 15, 15, 4, 13, 13, 13},
            {11, 15, 15, 15, 14, 10, 15, 15, 15, 1, 15, 15, 4, 7, 7, 7},
            {11, 7, 7, 7, 14, 10, 15, 15, 15, 1, 15, 15, 4, 11, 11, 11},
            {9, 12, 9, 12, 9, 12, 2, 2, 2, 0, 0, 0, 0, 14, 14, 14},
            {3, 6, 3, 6, 3, 6, 5, 5, 5, 0, 0, 0, 0, 0, 0, 0},
            {15, 15, 15, 15, 11, 14, 8, 10, 15, 15, 15, 15, 15, 0, 0, 0},
            {15, 15, 15, 15, 11, 14, 8, 10, 15, 15, 15, 15, 15, 0, 0, 0},
            {15, 15, 15, 15, 11, 14, 8, 10, 15, 15, 15, 15, 15, 0, 0, 0},
            {15, 15, 15, 15, 11, 14, 2, 10, 15, 15, 15, 15, 15, 0, 0, 0},
            {15, 15, 15, 15, 11, 14, 2, 10, 15, 15, 15, 15, 15, 0, 0, 0},
            {15, 15, 15, 15, 11, 14, 2, 10, 15, 15, 15, 15, 15, 0, 0, 0},
            {13, 13, 13, 13, 13, 13, 15, 15, 15, 5, 5, 5, 0, 0, 0, 0},
            {7, 7, 7, 7, 7, 7, 10, 9, 13, 12, 9, 13, 12, 9, 13, 12},
            {4, 4, 4, 1, 1, 1, 10, 11, 15, 14, 11, 15, 14, 11, 15, 14},
            {5, 5, 5, 5, 5, 5, 10, 3, 7, 6, 3, 7, 6, 3, 7, 6},
            {11, 14, 13, 13, 13, 9, 9, 9, 12, 12, 12, 15, 15, 15, 0, 0},
            {11, 14, 7, 7, 7, 3, 3, 3, 6, 6, 6, 15, 15, 15, 0, 0},
            {11, 14, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 0, 0},
            {13, 13, 13, 13, 13, 13, 15, 15, 15, 0, 0, 0, 0, 0, 0, 0},
            {7, 7, 7, 7, 7, 7, 15, 15, 15, 0, 0, 0, 0, 0, 0, 0},
            {11, 11, 11, 11, 11, 11, 15, 15, 15, 0, 0, 0, 0, 0, 0, 0},
            {14, 14, 14, 14, 14, 14, 15, 15, 15, 0, 0, 0, 0, 0, 0, 0}
        };

        //Make this class a singleton
        public static BlendRules Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new BlendRules();
                }
                return instance;
            }
        }

        /* The idea of blend rules is adapted from http://seancode.com/terrafirma/uvs.html which describes the process of determining the UV of a tile via a set of constraints
         * See the "MatchRule" object at the bottom for summaries
         * The general idea is that the rules are placed into 16 different "buckets" where each bucket is chosen based on what same-type neighbors a tile has
         */
        public BlendRules()
        {
            for (int i = 0; i < 16; i++)
            {
                baseRules[i] = new LinkedList<MatchRule>();
            }

            baseRules[0].AddFirst(new MatchRule(0x0000, "D10", "D12")); //None
            baseRules[1].AddFirst(new MatchRule(0x0000, "A10", "C10")); //Right
            baseRules[2].AddFirst(new MatchRule(0x0000, "D7", "D9")); //Top
            baseRules[3].AddFirst(new MatchRule(0x0000, "E1", "E5")); //Top, Right
            baseRules[4].AddFirst(new MatchRule(0x0000, "A13", "C13")); //Left
            baseRules[5].AddFirst(new MatchRule(0x0000, "E7", "E9")); //Left, Right
            baseRules[6].AddFirst(new MatchRule(0x0000, "E2", "E6")); //Left, Top
            baseRules[7].AddFirst(new MatchRule(0x0000, "C2", "C4")); //Left, Top, Right
            baseRules[8].AddFirst(new MatchRule(0x0000, "A7", "A9")); //Bottom
            baseRules[9].AddFirst(new MatchRule(0x0000, "D1", "D5")); //Bottom, Right
            baseRules[10].AddFirst(new MatchRule(0x0000, "A6", "C6")); //Bottom, Top
            baseRules[11].AddFirst(new MatchRule(0x0000, "A1", "C1")); //Bottom, Top, Right
            baseRules[12].AddFirst(new MatchRule(0x0000, "D2", "D6")); //Bottom, Left
            baseRules[13].AddFirst(new MatchRule(0x0000, "A2", "A4")); //Bottom, Left, Right
            baseRules[14].AddFirst(new MatchRule(0x0000, "A5", "C5")); //Bottom, Left, Top
            baseRules[15].AddFirst(new MatchRule(0x0000, "B2", "B4")); //Bottom, Left, Top, Right
            baseRules[15].AddFirst(new MatchRule(0x0110, "A11", "C11")); //Bottom, Left, Top, Right, !TL, !BL
            baseRules[15].AddFirst(new MatchRule(0x1001, "A12", "C12")); //Bottom, Left, Top, Right, !TR, !BR
            baseRules[15].AddFirst(new MatchRule(0x0011, "B7", "B9")); //Bottom, Left, Top, Right, !TL, !TR
            baseRules[15].AddFirst(new MatchRule(0x1100, "C7", "C9")); //Bottom, Left, Top, Right, !BL, !BR

            for (int i = 0; i < 16; i++)
            {
                //blendRules[i] = new LinkedList<MatchRule>(baseRules[i]); //These will be added later
                blendRules[i] = new LinkedList<MatchRule>();
            }

            blendRules[0].AddFirst(new MatchRule(0x0000, 0x00000001, 0x0000, "N4", "N6"));
            blendRules[0].AddFirst(new MatchRule(0x0000, 0x00000010, 0x0000, "I7", "K7"));
            blendRules[0].AddFirst(new MatchRule(0x0000, 0x00000100, 0x0000, "N1", "N3"));
            blendRules[0].AddFirst(new MatchRule(0x0000, 0x00001000, 0x0000, "F7", "H7"));
            blendRules[0].AddFirst(new MatchRule(0x0000, 0x00000101, 0x0000, "L10", "L12"));
            blendRules[0].AddFirst(new MatchRule(0x0000, 0x00001010, 0x0000, "M7", "O7"));
            blendRules[0].AddFirst(new MatchRule(0x0000, 0x00001111, 0x0000, "L7", "L9"));
            blendRules[1].AddFirst(new MatchRule(0x0000, 0x00000100, 0x0000, "O1", "O3"));
            blendRules[2].AddFirst(new MatchRule(0x0000, 0x00001000, 0x0000, "F8", "H8"));
            blendRules[3].AddFirst(new MatchRule(0x0000, 0x00000100, 0x0000, "M1", "M3"));
            blendRules[3].AddFirst(new MatchRule(0x0000, 0x00001000, 0x0000, "F5", "H5"));
            blendRules[3].AddFirst(new MatchRule(0x0000, 0x00001100, 0x0000, "G3", "K3"));
            blendRules[4].AddFirst(new MatchRule(0x0000, 0x00000001, 0x0000, "O4", "O6"));
            blendRules[5].AddFirst(new MatchRule(0x0000, 0x00001010, 0x0000, "K9", "K11"));
            blendRules[6].AddFirst(new MatchRule(0x0000, 0x00000001, 0x0000, "M4", "M6"));
            blendRules[6].AddFirst(new MatchRule(0x0000, 0x00001000, 0x0000, "F6", "H6"));
            blendRules[6].AddFirst(new MatchRule(0x0000, 0x00001001, 0x0000, "G4", "K4"));
            blendRules[7].AddFirst(new MatchRule(0x0000, 0x00001000, 0x0000, "F9", "F11"));
            blendRules[8].AddFirst(new MatchRule(0x0000, 0x00000010, 0x0000, "I8", "K8"));
            blendRules[9].AddFirst(new MatchRule(0x0000, 0x00000010, 0x0000, "I5", "K5"));
            blendRules[9].AddFirst(new MatchRule(0x0000, 0x00000100, 0x0000, "L1", "L3"));
            blendRules[9].AddFirst(new MatchRule(0x0000, 0x00000110, 0x0000, "F3", "J3"));
            blendRules[10].AddFirst(new MatchRule(0x0000, 0x00000101, 0x0000, "H11", "J11"));
            blendRules[11].AddFirst(new MatchRule(0x0000, 0x00000100, 0x0000, "H10", "J10"));
            blendRules[12].AddFirst(new MatchRule(0x0000, 0x00000001, 0x0000, "L4", "L6"));
            blendRules[12].AddFirst(new MatchRule(0x0000, 0x00000010, 0x0000, "I6", "K6"));
            blendRules[12].AddFirst(new MatchRule(0x0000, 0x00000011, 0x0000, "F4", "J4"));
            blendRules[13].AddFirst(new MatchRule(0x0000, 0x00000010, 0x0000, "G9", "G11"));
            blendRules[14].AddFirst(new MatchRule(0x0000, 0x00000001, 0x0000, "H9", "J9"));

            for (int i = 0; i < 16; i++)
            {
                grassRules[i] = new LinkedList<MatchRule>(blendRules[i]);
                for (int j = 0; j < baseRules[i].Count; j++)
                {
                    blendRules[i].AddLast(baseRules[i].ElementAt(j));
                }
            }
            //These rules cause conflicts with some of the other rules during grass blending
            grassRules[7].RemoveFirst();
            grassRules[11].RemoveFirst();
            grassRules[13].RemoveFirst();
            grassRules[14].RemoveFirst();
            grassRules[3].RemoveFirst();
            grassRules[6].RemoveFirst();
            grassRules[9].RemoveFirst();
            grassRules[12].RemoveFirst();

            blendRules[1].AddFirst(new MatchRule(0x0000, 0x00001110, 0x0000, "F13", "H13"));
            blendRules[2].AddFirst(new MatchRule(0x0000, 0x00001101, 0x0000, "I12", "K12"));
            blendRules[4].AddFirst(new MatchRule(0x0000, 0x00001011, 0x0000, "I13", "K13"));
            blendRules[5].AddFirst(new MatchRule(0x0000, 0x00000010, 0x0000, "B14", "B16"));
            blendRules[5].AddFirst(new MatchRule(0x0000, 0x00001000, 0x0000, "A14", "A16"));
            blendRules[8].AddFirst(new MatchRule(0x0000, 0x00000111, 0x0000, "F12", "H12"));
            blendRules[10].AddFirst(new MatchRule(0x0000, 0x00000001, 0x0000, "C14", "C16"));
            blendRules[10].AddFirst(new MatchRule(0x0000, 0x00000100, 0x0000, "D14", "D16"));
            blendRules[15].AddFirst(new MatchRule(0x0000, 0x00010000, 0x0000, "G1", "K1"));
            blendRules[15].AddFirst(new MatchRule(0x0000, 0x00100000, 0x0000, "G2", "K2"));
            blendRules[15].AddFirst(new MatchRule(0x0000, 0x01000000, 0x0000, "F2", "J2"));
            blendRules[15].AddFirst(new MatchRule(0x0000, 0x10000000, 0x0000, "F1", "J1"));

            grassRules[1].AddLast(new MatchRule(0x0000, 0x0000, 0x00001010, 0x00000000, "P1", "R1"));
            grassRules[1].AddLast(new MatchRule(0x0000, 0x0000, 0x00001110, 0x00000000, "R9", "R11"));
            grassRules[2].AddLast(new MatchRule(0x0000, 0x0000, 0x00000101, 0x00000000, "Q3", "Q5"));
            grassRules[2].AddLast(new MatchRule(0x0000, 0x0000, 0x00001101, 0x00000000, "Q12", "Q14"));
            grassRules[3].AddLast(new MatchRule(0x0000, 0x0001, 0x00000000, 0x00010000, "Q6", "Q8"));
            grassRules[4].AddLast(new MatchRule(0x0000, 0x0000, 0x00001010, 0x00000000, "P2", "R2"));
            grassRules[4].AddLast(new MatchRule(0x0000, 0x0000, 0x00001011, 0x00000000, "R12", "R14"));
            grassRules[6].AddLast(new MatchRule(0x0000, 0x0010, 0x00000000, 0x00100000, "Q9", "Q11"));
            grassRules[7].AddLast(new MatchRule(0x0000, 0x0011, 0x00000000, 0x00111000, "O9", "O15"));
            grassRules[7].AddLast(new MatchRule(0x0010, 0x0001, 0x00100000, 0x00011000, "T1", "T3"));
            grassRules[7].AddLast(new MatchRule(0x0001, 0x0010, 0x00010000, 0x00101000, "T4", "T6"));
            grassRules[8].AddLast(new MatchRule(0x0000, 0x0000, 0x00000101, 0x00000000, "P3", "P5"));
            grassRules[8].AddLast(new MatchRule(0x0000, 0x0000, 0x00000111, 0x00000000, "P12", "P14"));
            grassRules[9].AddLast(new MatchRule(0x0000, 0x1000, 0x00000000, 0x10000000, "P6", "P8"));
            grassRules[11].AddLast(new MatchRule(0x0000, 0x1001, 0x00000000, 0x10010100, "N8", "N14"));
            grassRules[11].AddLast(new MatchRule(0x1000, 0x0001, 0x10000000, 0x00010100, "U1", "U3"));
            grassRules[11].AddLast(new MatchRule(0x0001, 0x1000, 0x00010000, 0x10000100, "U4", "U6"));
            grassRules[12].AddLast(new MatchRule(0x0000, 0x0100, 0x00000000, 0x01000000, "P9", "P11"));
            grassRules[13].AddLast(new MatchRule(0x0000, 0x1100, 0x00000000, 0x11000010, "M9", "M15"));
            grassRules[13].AddLast(new MatchRule(0x0100, 0x1000, 0x01000000, 0x10000010, "S1", "S3"));
            grassRules[13].AddLast(new MatchRule(0x1000, 0x0100, 0x10000000, 0x01000010, "S4", "S6"));
            grassRules[14].AddLast(new MatchRule(0x0000, 0x0110, 0x00000000, 0x01100001, "N10", "N16"));
            grassRules[14].AddLast(new MatchRule(0x0010, 0x0100, 0x00100000, 0x01000001, "V1", "V3"));
            grassRules[14].AddLast(new MatchRule(0x0100, 0x0010, 0x01000000, 0x00100001, "V4", "V6"));
            grassRules[15].AddLast(new MatchRule(0x0000, 0x1111, 0x00000000, 0x11110000, "N9", "N15"));
            grassRules[15].AddLast(new MatchRule(0x0000, 0x0111, 0x10000000, 0x01110000, "S7", "S9"));
            grassRules[15].AddLast(new MatchRule(0x0000, 0x1110, 0x00010000, 0x11100000, "T7", "T9"));
            grassRules[15].AddLast(new MatchRule(0x0000, 0x1011, 0x01000000, 0x10110000, "U7", "U9"));
            grassRules[15].AddLast(new MatchRule(0x0000, 0x1101, 0x00100000, 0x11010000, "V7", "V9"));
            grassRules[15].AddLast(new MatchRule(0x0000, 0x1010, 0x00000000, 0x10100000, "R3", "R5"));
            grassRules[15].AddLast(new MatchRule(0x0000, 0x0101, 0x00000000, 0x01010000, "R6", "R8"));

            //These rules are not listed in the terrafirma guide, but are required for this system to completely work for some reason
            grassRules[0].AddFirst(new MatchRule(0x0000, 0x0000, 0x00001110, 0x00000001, "P2", "R2"));
            grassRules[0].AddFirst(new MatchRule(0x0000, 0x0000, 0x00001101, 0x00000010, "P3", "P5"));
            grassRules[0].AddFirst(new MatchRule(0x0000, 0x0000, 0x00001011, 0x00000100, "P1", "R1"));
            grassRules[0].AddFirst(new MatchRule(0x0000, 0x0000, 0x00000111, 0x00001000, "Q3", "Q5"));
            grassRules[1].AddFirst(new MatchRule(0x0000, 0x0000, 0x00000110, 0x00001001, "M1", "M3"));
            grassRules[1].AddFirst(new MatchRule(0x0000, 0x0000, 0x00001100, 0x00000011, "L1", "L3"));
            grassRules[2].AddFirst(new MatchRule(0x0000, 0x0000, 0x00001001, 0x00000110, "F5", "H5"));
            grassRules[2].AddFirst(new MatchRule(0x0000, 0x0000, 0x00001100, 0x00000011, "F6", "H6"));
            grassRules[3].AddFirst(new MatchRule(0x0000, 0x0001, 0x00001100, 0x00010000, "G3", "K3"));
            grassRules[4].AddFirst(new MatchRule(0x0000, 0x0000, 0x00000011, 0x00001100, "M4", "M6"));
            grassRules[4].AddFirst(new MatchRule(0x0000, 0x0000, 0x00001001, 0x00000110, "L4", "L6"));
            grassRules[6].AddFirst(new MatchRule(0x0000, 0x0010, 0x00001001, 0x00100000, "G4", "K4"));
            grassRules[7].AddLast(new MatchRule(0x0000, 0x0011, 0x00001000, 0x00110000, "B7", "B9"));
            grassRules[7].AddLast(new MatchRule(0x0000, 0x0001, 0x00001000, 0x00010000, "G3", "K3"));
            grassRules[7].AddLast(new MatchRule(0x0000, 0x0010, 0x00001000, 0x00100000, "G4", "K4"));
            grassRules[8].AddFirst(new MatchRule(0x0000, 0x0000, 0x00000011, 0x00001100, "I5", "K5"));
            grassRules[8].AddFirst(new MatchRule(0x0000, 0x0000, 0x00000110, 0x00001001, "I6", "K6"));
            grassRules[9].AddFirst(new MatchRule(0x0000, 0x1000, 0x00000110, 0x10000000, "F3", "J3"));
            grassRules[11].AddLast(new MatchRule(0x0000, 0x1001, 0x00000100, 0x10010000, "A12", "C12"));
            grassRules[11].AddLast(new MatchRule(0x0000, 0x0001, 0x00000100, 0x00010000, "G3", "K3"));
            grassRules[11].AddLast(new MatchRule(0x0000, 0x1000, 0x00000100, 0x10000000, "F3", "J3"));
            grassRules[12].AddFirst(new MatchRule(0x0000, 0x0100, 0x00000011, 0x01000000, "F4", "J4"));
            grassRules[13].AddLast(new MatchRule(0x0000, 0x1100, 0x00000010, 0x11000000, "C7", "C9"));
            grassRules[13].AddLast(new MatchRule(0x0000, 0x0100, 0x00000010, 0x01000000, "F4", "J4"));
            grassRules[13].AddLast(new MatchRule(0x0000, 0x1000, 0x00000010, 0x10000000, "F3", "J3"));
            grassRules[14].AddLast(new MatchRule(0x0000, 0x0110, 0x00000001, 0x01100000, "A11", "C11"));
            grassRules[14].AddLast(new MatchRule(0x0000, 0x0010, 0x00000001, 0x00100000, "G4", "K4"));
            grassRules[14].AddLast(new MatchRule(0x0000, 0x0100, 0x00000001, 0x01000000, "F4", "J4"));
            grassRules[15].AddLast(new MatchRule(0x0000, 0x0011, 0x00000000, 0x00110000, "B7", "B9"));
            grassRules[15].AddLast(new MatchRule(0x0000, 0x1100, 0x00000000, 0x11000000, "C7", "C9"));
            grassRules[15].AddLast(new MatchRule(0x0000, 0x0110, 0x00000000, 0x01100000, "A11", "C11"));
            grassRules[15].AddLast(new MatchRule(0x0000, 0x1001, 0x00000000, 0x10010000, "A12", "C12"));
            grassRules[15].AddLast(new MatchRule(0x0000, 0x0001, 0x00000000, 0x00010000, "G3", "K3"));
            grassRules[15].AddLast(new MatchRule(0x0000, 0x0010, 0x00000000, 0x00100000, "G4", "K4"));
            grassRules[15].AddLast(new MatchRule(0x0000, 0x0100, 0x00000000, 0x01000000, "F4", "J4"));
            grassRules[15].AddLast(new MatchRule(0x0000, 0x1000, 0x00000000, 0x10000000, "F3", "J3"));
        }

        //Given a set of masks and a strictness, follow the rules until the proper UV is obtained
        public Vector2Int32 GetUVForMasks(uint neighborMask, uint blendMask, int ruleStrictness)
        {
            int bucketId = (int)(((neighborMask & 0x00001000) >> 9) + ((neighborMask & 0x00000100) >> 6) + ((neighborMask & 0x00000010) >> 3) + (neighborMask & 0x00000001));
            int variationId = randomVariation.Next(3);
            switch (ruleStrictness)
            {
                case 0:
                    foreach (MatchRule rule in baseRules[bucketId])
                    {
                        if (rule.Matches(neighborMask, blendMask))
                        {
                            return rule.UVPosSet[variationId];
                        }
                    }
                    break;
                case 1:
                    foreach (MatchRule rule in blendRules[bucketId])
                    {
                        if (rule.Matches(neighborMask, blendMask))
                        {
                            return rule.UVPosSet[variationId];
                        }
                    }
                    break;
                case 2:
                    foreach (MatchRule rule in grassRules[bucketId])
                    {
                        if (rule.MatchesRelaxed(neighborMask, blendMask)) //Constraints behave slightly differently for grass tiles
                        {
                            return rule.UVPosSet[variationId];
                        }
                    }
                    //Only get here if there were no unique grass rules.  As a result, reduce the constraints even more
                    neighborMask |= blendMask;
                    bucketId = (int)(((neighborMask & 0x00001000) >> 9) + ((neighborMask & 0x00000100) >> 6) + ((neighborMask & 0x00000010) >> 3) + (neighborMask & 0x00000001));
                    foreach (MatchRule rule in baseRules[bucketId])
                    {
                        if (rule.Matches(neighborMask, blendMask))
                        {
                            return rule.UVPosSet[variationId];
                        }
                    }
                    break;
            }
            return new Vector2Int32(0, 0);
        }

        //This function resets the UV state for the specified tile locations (as well as nearby tiles) such that the UV cache must be re-evaluated
        public static void ResetUVCache(WorldViewModel _wvm, int tileStartX, int tileStartY, int regionWidth, int regionHeight)
        {
            if (_wvm.TilePicker.PaintMode == PaintMode.TileAndWall)
            {
                    //Reset UV Cache for nearby tiles and walls
                    for (int x = -1; x < regionWidth + 1; x++)
                    {
                        int tilex = x + tileStartX;
                        for (int y = -1; y < regionHeight + 1; y++)
                        {
                            int tiley = y + tileStartY;
                            if (tilex < 0 || tiley < 0 || tilex >= _wvm.CurrentWorld.TilesWide || tiley >= _wvm.CurrentWorld.TilesHigh)
                            {
                                continue;
                            }
                            Tile curtile = _wvm.CurrentWorld.Tiles[tilex, tiley];
                            if (_wvm.TilePicker.TileStyleActive)
                            {
                                curtile.uvTileCache = 0xFFFF;
                                curtile.lazyMergeId = 0xFF;
                                curtile.hasLazyChecked = false;
                            }
                            if (_wvm.TilePicker.WallStyleActive)
                                curtile.uvWallCache = 0xFFFF;
                        }
                    }
            }
        }
    }

    class MatchRule
    {
        //Corner order -> 0x0001 = Top Right, 0x0010 = Top Left, 0x0100 = Bottom Left, 0x1000 = Bottom Right
        //Edge order -> 0x0001 = Right (East), 0x0010 = Above (North), 0x0100 = Left (West), 0x1000 = Below (South)
        private int cornerInclusionMask = 0; //Specifies which corner tiles MUST match a given tile's type using the "corner order" above
        private int cornerExclusionMask = 0; //Specifies which corner tile must NOT match a given tile's type using the "corner order" above
        private long blendInclusionMask = 0; //When merging a tile to another (e.g. stone and dirt,) these tiles MUST match the merge type - 0xFFFF0000 refers to the "corner order" above, 0x0000FFFF refers to the "edge order" above
        private long blendExclusionMask = 0; //When merging a tile to another (e.g. stone and dirt,) these tiles must NOT match the merge type - 0xFFFF0000 refers to the "corner order" above, 0x0000FFFF refers to the "edge order" above
        private Vector2Int32[] uvPos; //A set of UV positions, each representing a possible variation of the rule's final position

        public MatchRule(int cornerExclusionMask, string tileStart, string tileEnd)
        {
            this.cornerExclusionMask = cornerExclusionMask;
            SetUVsForString(tileStart, tileEnd);
        }

        public MatchRule(int cornerExclusionMask, long blendInclusionMask, int blendCornerExclusionMask, string tileStart, string tileEnd)
        {
            this.cornerExclusionMask = cornerExclusionMask;
            this.blendInclusionMask = blendInclusionMask;
            blendExclusionMask = blendCornerExclusionMask << 16;
            SetUVsForString(tileStart, tileEnd);
        }

        public MatchRule(int cornerInclusionMask, int cornerExclusionMask, long blendInclusionMask, long blendExclusionMask, string tileStart, string tileEnd)
        {
            this.cornerInclusionMask = cornerInclusionMask;
            this.cornerExclusionMask = cornerExclusionMask;
            this.blendInclusionMask = blendInclusionMask;
            this.blendExclusionMask = blendExclusionMask;
            SetUVsForString(tileStart, tileEnd);
        }

        //tileStart and tileEnd use the "A1" notation based on the pictoral representation of tiles in http://seancode.com/terrafirma/uvs.html
        //Note that http://seancode.com/terrafirma/uvs.html does not list "N" as a possible row, but the rules used here do (and have been adjusted accordingly)
        private void SetUVsForString(string tileStart, string tileEnd)
        {
            uvPos = new Vector2Int32[3];
            int y1 = tileStart[0] - 'A';
            int x1 = Convert.ToInt32(tileStart.Substring(1)) - 1;
            int y2 = tileEnd[0] - 'A';
            int x2 = Convert.ToInt32(tileEnd.Substring(1)) - 1;
            int y3 = y2 - (y2 - y1) / 2;
            int x3 = x2 - (x2 - x1) / 2;
            uvPos[0] = new Vector2Int32(x1, y1);
            uvPos[1] = new Vector2Int32(x3, y3);
            uvPos[2] = new Vector2Int32(x2, y2);
        }

        //Works a "bit" of bit-magic to validate rules
        public bool Matches(long neighborMask, long blendMask)
        {
            long upperCornerInclusionMask = (cornerInclusionMask << 16) & 0x11110000;
            if ((upperCornerInclusionMask & neighborMask) != upperCornerInclusionMask)
            {
                return false;
            }
            long upperCornerExclusionMask = (cornerExclusionMask << 16) & 0x11110000;
            if (upperCornerExclusionMask != 0 && (upperCornerExclusionMask & neighborMask) != 0x00000000)
            {
                return false;
            }
            long lowerBlendInclusionMask = blendInclusionMask & 0x00001111;
            if (lowerBlendInclusionMask != 0 && (lowerBlendInclusionMask ^ (blendMask & 0x00001111)) != 0x00000000)
            {
                return false;
            }
            long upperBlendCornerInclusionMask = blendInclusionMask & 0x11110000;
            if ((upperBlendCornerInclusionMask & blendMask) != upperBlendCornerInclusionMask)
            {
                return false;
            }
            long lowerBlendExclusionMask = blendExclusionMask & 0x00001111;
            if ((lowerBlendExclusionMask & blendMask) != 0x00000000)
            {
                return false;
            }
            long upperBlendCornerExclusionMask = blendExclusionMask & 0x11110000;
            if (upperBlendCornerExclusionMask != 0 && (upperBlendCornerExclusionMask & blendMask) != 0x00000000)
            {
                return false;
            }
            return true;
        }

        //Same as above, inclusion masks are OR'd together on a bit-by-bit basis
        public bool MatchesRelaxed(long neighborMask, long blendMask)
        {
            long column = 0x00010000;
            for (int i = 0; i < 4; i++)
            {
                long upperCornerInclusionMask = (cornerInclusionMask << 16) & column;
                long upperBlendCornerInclusionMask = blendInclusionMask & column;
                if ((upperCornerInclusionMask & upperBlendCornerInclusionMask) == 0x00000000)
                {
                    if (upperCornerInclusionMask != 0 && (upperCornerInclusionMask & neighborMask) == 0)
                    {
                        return false;
                    }
                    if (upperBlendCornerInclusionMask != 0 && (upperBlendCornerInclusionMask & blendMask) == 0)
                    {
                        return false;
                    }
                }
                else
                {
                    if ((upperCornerInclusionMask & neighborMask) == 0 && (upperBlendCornerInclusionMask & blendMask) == 0)
                    {
                        return false;
                    }
                }
                if (i < 3)
                {
                    column <<= 4;
                }
            }
            long upperCornerExclusionMask = (cornerExclusionMask << 16) & 0x11110000;
            if (upperCornerExclusionMask != 0 && (upperCornerExclusionMask & neighborMask) != 0x00000000)
            {
                return false;
            }
            long lowerBlendInclusionMask = blendInclusionMask & 0x00001111;
            if (lowerBlendInclusionMask != 0 && (lowerBlendInclusionMask ^ (blendMask & 0x00001111)) != 0x00000000)
            {
                return false;
            }
            long lowerBlendExclusionMask = blendExclusionMask & 0x00001111;
            if ((lowerBlendExclusionMask & blendMask) != 0x00000000)
            {
                return false;
            }
            long upperBlendCornerExclusionMask = blendExclusionMask & 0x11110000;
            if (upperBlendCornerExclusionMask != 0 && (upperBlendCornerExclusionMask & blendMask) != 0x00000000)
            {
                return false;
            }
            return true;
        }

        public Vector2Int32[] UVPosSet
        {
            get { return uvPos; }
        }
    }
}
