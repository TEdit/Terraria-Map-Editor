
void FUN_005ec460(void)

{
  int iVar1;
  int *piVar2;
  int iVar3;
  int iVar4;
  undefined8 uVar5;
  char local_28 [4];
  
  uVar5 = FUN_0082a0b8();
  iVar1 = DAT_005ec4e8;
  piVar2 = (int *)uVar5;
  iVar3 = 0;
  do {
    iVar4 = iVar3 + DAT_00955b44;
    (**(code **)(*piVar2 + 0x24))(piVar2,local_28,1);
    if (local_28[0] == '\0') {
      *(undefined1 *)(iVar4 + 0x1ae4) = 1;
    }
    else {
      FUN_005ea37c(iVar4);
      *(undefined1 *)(iVar4 + 0x1ae4) = 0;
      (**(code **)(*piVar2 + 0x24))(piVar2,iVar4 + 0x1ae0,2);
      (**(code **)(*piVar2 + 0x24))(piVar2,iVar4 + 0x1ae2,2);
      FUN_005e8cac(iVar4,piVar2,(int)((ulonglong)uVar5 >> 0x20));
    }
    iVar3 = iVar3 + 0x1aec;
  } while (iVar3 < iVar1);
  FUN_0082a0d0();
  return;
}

