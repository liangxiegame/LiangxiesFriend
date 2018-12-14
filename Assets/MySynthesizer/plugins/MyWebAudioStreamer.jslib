
var MyWebAudioStreamerPlugin = {
	$my: null,

	MyWebAudioStreamerStart: function(bufferCount, bufferLength, sampleRate)
	{
		console.log("MyWebAudioStreamerStart bufferCount=" + bufferCount + " bufferLength=" + bufferLength + " sampleRate=" + sampleRate);
		if((my != null) && (my != undefined))
		{
			return;
		}
		var ctxt = WEBAudio.audioContext;//new (window.AudioContext || window.webkitAudioContext);
		my = this;
		my.ctxt = ctxt;
		my.bufferLength = bufferLength;
		my.bufferIndex = 0;
		my.audioBuffers = new Array(bufferCount);
		for (var i = 0; i < bufferCount; i++)
		{
			my.audioBuffers[i] = ctxt.createBuffer(2, bufferLength, sampleRate);
		}
		my.finishTimes = new Array(bufferCount);
		my.work0 = new Float32Array(bufferLength);
		my.work1 = new Float32Array(bufferLength);
		my.scheduledTime = ctxt.currentTime;
		my.durationTime = bufferLength / sampleRate;
		my.play = function() {
			var ctxt = my.ctxt;
			var index = my.bufferIndex;
			var buffer = my.audioBuffers[index];
			var source = ctxt.createBufferSource();
			source.buffer = buffer;
			source.connect(ctxt.destination);
			//source.onended = my.update;
			var scheduledTime = my.scheduledTime;
			var currentTime = ctxt.currentTime;
			if(scheduledTime <= currentTime)
			{
				scheduledTime = currentTime + my.durationTime;
			}
			if (source.start)
			{
				source.start(scheduledTime);
			}
			else
			{
				source.noteOn(scheduledTime);
			}
			var finishTime = scheduledTime + my.durationTime;
			my.scheduledTime = finishTime;
			my.finishTimes[index] = finishTime;
			index++;
			if(index >= my.audioBuffers.length)
			{
				index = 0;
			}
			my.bufferIndex = index;
		}

		my.scheduledTime = ctxt.currentTime + my.durationTime;
		for (var i = 0; i < bufferCount; i++)
		{
			my.play();
		}
		console.log("MyWebAudioStreamerStart: started");
	},

	MyWebAudioStreamerUpdate: function(bufferPtr)
	{
		if((my == null) || (my == undefined))
		{
			return false;
		}
		var ctxt = my.ctxt;
		var currentTime = ctxt.currentTime;
		var finishTime = my.finishTimes[my.bufferIndex];
		if(finishTime > currentTime)
		{
			return false;
		}
		if(bufferPtr == 0)
		{
			return true;
		}
		var len = my.bufferLength;
		var index = bufferPtr / HEAPF32.BYTES_PER_ELEMENT;
		var bufferAry = HEAPF32.subarray(index, index + len * 2);
		var work0 = my.work0;
		var work1 = my.work1;
		for(var i = 0; i < len; i++)
		{
			work0[i] = bufferAry[i * 2 + 0];
			work1[i] = bufferAry[i * 2 + 1];
		}
		var buffer = my.audioBuffers[my.bufferIndex];
		if(buffer.copyToChannel)
		{
			buffer.copyToChannel(work0, 0);
			buffer.copyToChannel(work1, 1);
		}
		else
		{
			buffer.getChannelData(0).set(work0);
			buffer.getChannelData(1).set(work1);
		}
		my.play();

		return (my.finishTimes[my.bufferIndex] <= currentTime);
	},

	MyWebAudioStreamerStop: function()
	{
		if((my == null) || (my == undefined))
		{
			return;
		}
		_free(my.bufferPtr);
		my.ctxt = null;
		my.bufferPtr = null;
		my.bufferArr = null;
		my.action = null;
		my.audioBuffers = null;
		my.finishTimes = null;
		my.work0 = null;
		my.work1 = null;
		my = null;
		console.log("MyWebAudioStreamerStop: stopped");
	},
}

autoAddDeps(MyWebAudioStreamerPlugin, '$my');
mergeInto(LibraryManager.library, MyWebAudioStreamerPlugin);

