   M S B A M L   `   `   `  ����   RemoteRecorderDemoGUI&     RemoteRecorderDemoGUI.MainWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml       � Title$  Remote Recorder Demo��$	��450��$	��500�� � 
ResizeMode$ CanMinimize=� � Icon$ Logo-PanoptoGlobe.png��.���� ���	#FF259216$��3q�._�� $��0,0,0,0q�.+��� 
����ح�$��
User Login��$	��Top=�$	��5,0q�$	��2.5q�.��� .+��� $��
User Name:��$
��Left=�$��25��$��	10,10,0,0q�$	��Top=� J� RenderTransformOrigin$ 0.5,0.5&��� -    ��Name$ UserID��$��
120,10,0,0q� �� TextWrapping$
 Wrap=�$	��Foo��$	��Top=�$	��300��$��25��$
��Left=��� $��	Password:��$
��Left=�$��25��$��	10,45,0,0q�$	��Top=�G� -    G�Name$ UserPassword��$��25��$��
120,45,0,0q�$	��Top=�$	��300�� G� Password$	 Bar��$
��Left=��� -    ��Name$ LoginLogout��$��Login��$��30��$	��120��$	��Top=�$��	0,80,0,10q��� 
����ح�$��Remote Recording Options��$	��Top=�$��	5,150,5,0q�$	��2.5q�$��0,0,0,10q�.��� -   	 �Name$	 	RROptions��$��0q�.+��� -   
 ��Name$
 SessionNameLabel��$��Session Name:��$
��Left=�$��30��$��	10,10,0,0q�$	��Top=�$y�Hidden=��� -   $ SessionName��$��25��$��
120,10,0,0q�$
 Wrap=�$	��Bar��$	��Top=�$	��300��$
��Left=�$y�Hidden=��� -   $
 SessionLengthLabel��$��Session Length:��$
��Left=�$��30��$��	10,45,0,0q�$	��Top=�$y�Hidden=��� -   $ SessionLength��$��25��$��
120,45,0,0q�$
 Wrap=�$��50��$	��Top=�$	��250��$
��Left=� �� MaxLines$ 1�� �� 	MaxLength$ 9��$y�Hidden=��� -	   $
 SessionLengthUnit��$��25��$��Minutes��$
��Left=�$	��Top=�$��
370,45,0,0q�$y�Hidden=��� -
   $
 FolderLabel��$��25��$��Folder:��$
��Left=�$��	10,80,0,0q�$	��Top=�$y�Hidden=��� -    ��Name$ Folder��$��25��$
��Left=�$	��Top=�$��
120,80,0,0q�$	��300��$y�Hidden=��� -   $
 RecorderLabel��$��Remote Recorder:��$	��Top=�$
��Left=�$��25��$��
10,115,0,0q�$y�Hidden=��� -   $ RemoteRecorder��$��25��$
��Left=�$	��Top=�$	��300��$��120,115,0,0q�$y�Hidden=��� -   $ 	StartStop��$��Start Recording��$��30��$��0,0,0,50q�$��Bottom=�$	��120��$y�Hidden=��� -    ��Name$ 	StatusBar��$��35��$��0q�$��Bottom=�.@��� -   $
 Status��$��Please Login��