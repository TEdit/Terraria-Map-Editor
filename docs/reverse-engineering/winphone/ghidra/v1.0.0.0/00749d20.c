
void FUN_00749d20(int *param_1,int param_2)

{
  uint uVar1;
  uint in_fpscr;
  float fVar2;
  
  if (0x39 < param_2) {
    (**(code **)(*param_1 + 0x1c))(param_1,&DAT_010299a0,1);
    (**(code **)(*param_1 + 0x1c))(param_1,&DAT_010299a4,4);
    (**(code **)(*param_1 + 0x1c))(param_1,&DAT_010299a8,4);
    (**(code **)(*param_1 + 0x1c))(param_1,&DAT_010299b4,4);
    return;
  }
  uVar1 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
  DAT_00fd67f4 = DAT_00fd67f8;
  DAT_00fd67f8 = DAT_00fd67fc;
  uVar1 = DAT_00fd6800 ^ (uVar1 ^ DAT_00fd6800 >> 0xb) >> 8 ^ uVar1;
  DAT_00fd67fc = DAT_00fd6800;
  DAT_00fd6800 = uVar1;
  fVar2 = (float)VectorSignedToFloat(uVar1 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
  DAT_010299b4 = VectorSignedToFloat(DAT_00749ddc - (int)(fVar2 * DAT_00749dd8 * DAT_00749dd4),
                                     (byte)(in_fpscr >> 0x16) & 3);
  return;
}

