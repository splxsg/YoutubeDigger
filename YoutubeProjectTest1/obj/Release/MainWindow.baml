   M S B A M L   `   `   `  ���� N  JYoutubeProjectTest1, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null9!clr-namespace:YoutubeProjectTest1YoutubeProjectTest1  $    YoutubeProjectTest1.MainWindow   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     3local!clr-namespace:YoutubeProjectTest1     -     � Title$  
MainWindow��$	��900��$
��1600��$
��1920��$	��800��$
��1080��$	��450��.��� .+��� -    ��Name$ 	searchbtn��$��search��$
��Left=�$��37��$��
630,22,0,0q�$	��Top=�$��92���� -    ��Name$ 	keywordtb��$
��Left=�$��20��$��
111,37,0,0q� �� TextWrapping$
 Wrap=�$�� ��$	��Top=�$	��485���� -    ��Name$ label��$��Keyword:��$
��Left=�$��	43,33,0,0q�$	��Top=��� -    ��Name$ 
resultList��$��Center=�$	��720��$��0q�$��Center=�$
��1440�����	#FF06080E J� OpacityMask ��  �� Color$ 	#FF4F2222�� �� Opacity$ 0.995Q����� 	 �� EndPoint$	 0.5,1&�
 �� 
StartPoint$
 0.5,0&�.���  � Color$ Black�� � Offset$ 0Q�� $ 	#FF0A0505��$ 1Q� �� View �� .P��� $��Channel�� �� DisplayMemberBinding �� *	channel+�� $��Video Title�� �� *title+�� $��
View Count�� �� *count+ �� StringFormat$ N2���� $��
Subscriber�� �� *
subscriber+�� $��Average View�� �� *avgviewcount+�� $��Emails�� �� *email+�� -   $ loadmorebtn��$��	Load more��$��Center=�$��0,0,0,20q�$��Bottom=�$	��141��$��38�����	#FF707070�� -   $ Export��$��Export to Excel��$
��Left=�$��
776,22,0,0q�$	��Top=�$	��102��$��37��